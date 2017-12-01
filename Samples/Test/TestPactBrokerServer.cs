using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace Aqovia.PactProducerVerifier.Sample.Test
{
    internal class TestPactBrokerServer : IDisposable
    {
        private readonly int _port;
        private readonly string _branchName;
        private HttpServer _httpServer;

        public TestPactBrokerServer(int port, string branchName)
        {
            _port = port;
            _branchName = branchName;
        }

        public void Start()
        {
            _httpServer = new HttpServer(new HttpRequestProvider());
            _httpServer.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Loopback, _port)));

            _httpServer.Use(new InlineHandler(context =>
                {
                    if (context.Request.Uri.OriginalString == "/pacts/provider/PactProducerSampleTests/latest")
                    {
                        context.Response = new HttpResponse(HttpResponseCode.Ok, @"{'_links':{'pacts':[{'href':'http://localhost:" + _port + @"/pacts/provider/PactProducerSampleTests/consumer/testPact/latest/" + _branchName + @"','name':'testPact'}]}}", false);
                    }
                    else if (context.Request.Uri.OriginalString == "/pacts/provider/PactProducerSampleTests/consumer/testPact/latest/" + _branchName)
                    {
                        context.Response = new HttpResponse(HttpResponseCode.Ok, @"{
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
                        }", false);
                    }
                    else
                    {
                        context.Response = new HttpResponse(HttpResponseCode.NotFound, "Not found", false);
                    }
                    return Task.CompletedTask;
                }));

            _httpServer.Start();                            
        }

        public void Dispose()
        {
            _httpServer?.Dispose();
        }
    }
}
