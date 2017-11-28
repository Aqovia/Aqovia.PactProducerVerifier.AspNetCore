using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Aqovia.PactProducerVerifier.Api;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Headers;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;
using Xunit;
using Xunit.Abstractions;


namespace Aqovia.PactProducerVerifier.Sample
{    
    public class PactProducerSampleTestsXUnit
    {
        private readonly PactProducerTests _pactProducerTests;
        private const int TeamCityMaxBranchLength = 19;
        public PactProducerSampleTestsXUnit(ITestOutputHelper output)
        {
            ProducerVerifierConfiguration configuration = new ProducerVerifierConfiguration
            {
                TeamCityProjectName = "PactProducerSampleTests",
                PactBrokerUri = "http://localhost:13800",
                AspNetCoreStartup = typeof(Startup),
                StartupAssemblyLocation = Path.Combine(TestContext.CurrentContext.TestDirectory, "..\\..\\..\\..\\..\\Aqovia.PactProducerVerifier.Api")
            };
            _pactProducerTests = new PactProducerTests(configuration, output.WriteLine, ThisAssembly.Git.Branch, builder =>
            {
                builder.UseMiddleware(typeof(TestStateProvider));

            }, TeamCityMaxBranchLength);
        }

        [Fact]                
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



}
