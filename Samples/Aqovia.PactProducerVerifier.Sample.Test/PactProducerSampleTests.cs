using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Aqovia.PactProducerVerifier.Api;
using Microsoft.AspNetCore.Builder;
using uhttpsharp;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace Aqovia.PactProducerVerifier.Sample.Test
{        
    public class PactProducerSampleTests
    {
        private readonly PactProducerTests _pactProducerTests;
        private const int TeamCityMaxBranchLength = 19;
        public PactProducerSampleTests()
        {
            ProducerVerifierConfiguration configuration= new ProducerVerifierConfiguration
            {
                TeamCityProjectName = "PactProducerSampleTests",
                PactBrokerUri = "http://localhost:13800",
                AspNetCoreStartup = typeof(Startup)
            };
            _pactProducerTests = new PactProducerTests(configuration,  Console.WriteLine, ThisAssembly.Git.Branch, builder =>
            {
                builder.UseMiddleware(typeof(TestStateProvider));

            }, TeamCityMaxBranchLength);
        }

        //[Fact]        
        //[Test]
        public async Task EnsureApiHonoursPactWithConsumers()
        {
            using (var httpServer = new HttpServer(new HttpRequestProvider()))
            {                
                httpServer.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Loopback, 13800)));
                
                httpServer.Use(new InlineHandler(context =>
                {
                    if (context.Request.Uri.OriginalString == "/pacts/provider/PactProducerSampleTests/latest")
                    {
                        context.Response = new HttpResponse(HttpResponseCode.Ok, @"{'_links':{'pacts':[{'href':'http://localhost:13800/pacts/provider/PactProducerSampleTests/consumer/testPact/latest/OwinToAspNetCore','name':'testPact'}]}}", false);
                    }
                    else if (context.Request.Uri.OriginalString == "/pacts/provider/PactProducerSampleTests/consumer/testPact/latest/OwinToAspNetCore")
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
                                
                httpServer.Start();
                
                await _pactProducerTests.EnsureApiHonoursPactWithConsumersAsync();                
            }
        }
    }

    internal class InlineHandler : IHttpRequestHandler
    {
        private readonly Func<IHttpContext, Task> _handler;

        public InlineHandler(Func<IHttpContext,Task> handler)
        {
            _handler = handler;
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            return _handler(context);
        }
    }


}
