using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Aqovia.PactProducerVerifier.Sample.Test
{
    internal class TestPactBrokerServer : IDisposable
    {
        private readonly int _port;
        private readonly string _branchName;
        private Task _server;

        public TestPactBrokerServer(int port, string branchName)
        {
            _port = port;
            _branchName = branchName;
        }

        public void Start()
        {
            string url = $"http://{IPAddress.Loopback.ToString()}:{_port}";
            _server = WebHost.CreateDefaultBuilder(new string[] { })
            .UseUrls(url)
            .Configure(app =>
            {
                app.Run(async context =>
                {
                    if (context.Request.Path.Value == "/pacts/provider/PactProducerSampleTests/latest")
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync($@"{{'_links':{{'pacts':[{{'href':'{url}/pacts/provider/PactProducerSampleTests/consumer/testPact/latest/{_branchName}','name':'testPact'}}]}}}}");
                    }
                    else if (context.Request.Path.Value == "/pacts/provider/PactProducerSampleTests/consumer/testPact/latest/" + _branchName)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(@"{
                                          ""consumer"": {
                                            ""name"": ""Event API Consumer""
                                          },
                                          ""provider"": {
                                            ""name"": ""Event API""
                                          },
                                          ""interactions"": [
                                            {
                                              ""description"": ""should return values"",
                                              ""request"": {
                                                ""method"": ""get"",
                                                ""path"": ""/values/get"",
                                                ""headers"": {
                                                  ""Accept"": ""application/json""
                                                }
                                              },
                                              ""response"": {
                                                ""status"": 200,
                                                ""headers"": {
                                                  ""Content-Type"": ""application/json; charset=utf-8""
                                                },
                                                ""body"":  [""value1"",""value2""]                                
                                            }
                                        }      
                                          ],
                                          ""metadata"": {
                                            ""pactSpecification"": {
                                              ""version"": ""2.0.0""
                                            }
                                          }
                                        }");
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await context.Response.WriteAsync(string.Empty);
                    }
                });
            })
            .Build().StartAsync();
        }

        public void Dispose()
        {
            _server.Dispose();
        }
    }
}
