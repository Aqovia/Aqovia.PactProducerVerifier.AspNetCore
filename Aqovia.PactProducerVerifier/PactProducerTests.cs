﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PactNet;
using PactNet.Infrastructure.Outputters;

namespace Aqovia.PactProducerVerifier
{
    public class PactProducerTests : IDisposable
    {
        private const string MasterBranchName = "master";        
        private const string BaseServiceUri = "http://localhost";        
        //private readonly object _startup;
        //private readonly MethodInfo _method;
        private readonly ActionOutput _output;
        private readonly ProducerVerifierConfiguration _configuration;
        private readonly string _gitBranchName;
        private readonly Action<IApplicationBuilder> _onWebAppStarting;
        private readonly int _maxBranchNameLength;


        //Expose httpclient so that tests can supply a mocked version
        public HttpClient CurrentHttpClient;

        public PactProducerTests(ProducerVerifierConfiguration configuration, Action<string> output, string gitBranchName, Action<IApplicationBuilder> onWebAppStarting = null, int maxBranchNameLength = int.MaxValue)
        {
            _output = new ActionOutput(output);
            _configuration = configuration;
            _gitBranchName = gitBranchName;
            _onWebAppStarting = onWebAppStarting;
            _maxBranchNameLength = maxBranchNameLength;

            if (string.IsNullOrEmpty(configuration.TeamCityProjectName))
            {
                throw new ArgumentException($"App setting '{nameof(configuration.TeamCityProjectName)}' is missing or not set");
            }

            if (string.IsNullOrEmpty(configuration.PactBrokerUri))
            {
                throw new ArgumentException($"App setting '{nameof(configuration.PactBrokerUri)}' is missing or not set");
            }
            

            CurrentHttpClient = new HttpClient();

            //var path = AppDomain.CurrentDomain.BaseDirectory;
            //_startup = Activator.CreateInstance(configuration.AspNetCoreStartup);
            //_method = configuration.AspNetCoreStartup.GetMethod("Configuration");
        }

        public async Task EnsureApiHonoursPactWithConsumersAsync()
        {
            SetupRestClient();

            const int maxRetries = 5;
            var random = new Random();
            var uriBuilder = new UriBuilder(BaseServiceUri);
            for (var i = 0; i < maxRetries; i++)
            {
                try
                {
                    uriBuilder.Port = random.Next(10000, 20000);
                    await EnsureApiHonoursPactWithConsumersAsync(uriBuilder.Uri);
                    break;
                }
                catch (HttpListenerException ex)
                {
                    _output.WriteLine($"Service Uri: {uriBuilder.Uri.AbsoluteUri} failed with: {ex.Message}");
                    if(i < maxRetries)
                        _output.WriteLine("will retry ...");
                }
            }                        
        }

        private async Task EnsureApiHonoursPactWithConsumersAsync(Uri uri)
        {

            //var builder = new WebHostBuilder()
            //    .UseKestrel()
            //    .UseContentRoot(Directory.GetCurrentDirectory())
            //    .ConfigureAppConfiguration((hostingContext, config) => { /* setup config */  })
            //    .ConfigureLogging((hostingContext, logging) => { /* setup logging */  })
            //    .UseIISIntegration()
            //    .UseDefaultServiceProvider((context, options) => { /* setup the DI container to use */  })
            //    .ConfigureServices(services =>
            //    {
            //        services.AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelServerOptionsSetup>();
            //    });

            var customStartup = new TestStartup(_configuration.AspNetCoreStartup, _onWebAppStarting);

            using (var host = WebHost.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IStartup>(customStartup);                    
                })
                //.UseStartup(_configuration.AspNetCoreStartup)                
                .UseUrls(uri.AbsoluteUri)
                .UseSetting(WebHostDefaults.ApplicationKey, _configuration.AspNetCoreStartup.Assembly.FullName)
                .Build())
            {
                
                await host.StartAsync();

                var consumers = await GetConsumersAsync(CurrentHttpClient);
                var currentBranchName = GetCurrentBranchName();
                foreach (var consumer in consumers)
                {
                    var pactUrl = GetPactUrl(consumer, currentBranchName);
                    var pact = await CurrentHttpClient.GetAsync(pactUrl);
                    if (pact.StatusCode != HttpStatusCode.OK)
                    {
                        _output.WriteLine($"Pact does not exist for branch: {currentBranchName}, using {MasterBranchName} instead");
                        pactUrl = GetPactUrl(consumer, MasterBranchName);
                        pact = await CurrentHttpClient.GetAsync(pactUrl);
                        if (pact.StatusCode != HttpStatusCode.OK)
                            continue;
                    }
                    VerifyPactWithConsumer(consumer, pactUrl, uri);
                }
                await host.StopAsync();
            }
        }

        private string GetPactUrl(JToken consumer, string branchName)
        {
            return $"pacts/provider/{_configuration.TeamCityProjectName}/consumer/{consumer}/latest/{branchName}";
        }

        private async Task<IEnumerable<JToken>> GetConsumersAsync(HttpClient client)
        {            
            IEnumerable<JToken> consumers = new List<JToken>();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{_configuration.PactBrokerUri}/pacts/provider/{_configuration.TeamCityProjectName}/latest"),
                Method = HttpMethod.Get,               
            };
            //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(""));
            

            var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
                var latestPacts = (JArray)json["_links"]["pacts"];
                consumers = latestPacts.Select(s => s.SelectToken("name"));
            }
            return consumers;
        }

        private void SetupRestClient()
        {
            CurrentHttpClient.BaseAddress = new Uri(_configuration.PactBrokerUri);            
            var byteArray = Encoding.ASCII.GetBytes($"{_configuration.PactBrokerUsername}:{_configuration.PactBrokerPassword}");
            CurrentHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));            
        }
        private string GetCurrentBranchName()
        {
            var componentBranch = Environment.GetEnvironmentVariable("ComponentBranch");

            _output.WriteLine($"GitBranchName = {_gitBranchName}");
            _output.WriteLine($"Environment Variable 'ComponentBranch' = {componentBranch}");
            
            var branchName = _gitBranchName;
            branchName = string.IsNullOrEmpty(componentBranch) ? branchName : componentBranch;
            branchName = string.IsNullOrEmpty(branchName) ? MasterBranchName : branchName;

            branchName = branchName?.TrimStart('-').Length > _maxBranchNameLength ? 
                 branchName.TrimStart('-').Substring(0, _maxBranchNameLength)
                : branchName.TrimStart('-');

            _output.WriteLine($"Calculated BranchName = {branchName}");

            return branchName;
        }

        private void VerifyPactWithConsumer(JToken consumer, string pactUrl, Uri serviceUri)
        {
            //we need to instantiate one pact verifier for each consumer

            var config = new PactVerifierConfig
            {
                Outputters = new List<IOutput> 
                {
                    _output
                }
            };

            PactUriOptions pactUriOptions = null;
            if (!string.IsNullOrEmpty(_configuration.PactBrokerUsername))
                pactUriOptions = new PactUriOptions(_configuration.PactBrokerUsername, _configuration.PactBrokerPassword);

            var pactUri = new Uri(new Uri(_configuration.PactBrokerUri), pactUrl);
            var pactVerifier = new PactVerifier(config);

            pactVerifier
                .ProviderState(new Uri(serviceUri, "/provider-states").AbsoluteUri)
                .ServiceProvider(_configuration.TeamCityProjectName, serviceUri.AbsoluteUri)
                .HonoursPactWith(consumer.ToString())
                
                .PactUri(pactUri.AbsoluteUri, pactUriOptions)
                .Verify();
        }
        private class ActionOutput : IOutput
        {
            private readonly Action<string> _output;

            public ActionOutput(Action<string> output)
            {
                _output = output;
            }

            public void WriteLine(string line)
            {
                _output.Invoke(line);
            }
        }

        public void Dispose()
        {
            CurrentHttpClient?.Dispose();
        }
    }


}
