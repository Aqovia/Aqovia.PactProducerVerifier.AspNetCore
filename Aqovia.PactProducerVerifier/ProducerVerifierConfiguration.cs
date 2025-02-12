using System;
using Microsoft.AspNetCore.Hosting;

namespace Aqovia.PactProducerVerifier.AspNetCore
{
    public class ProducerVerifierConfiguration
    {
        /// <summary>
        /// The name of the provider
        /// </summary>
        public string ProviderName { get; set; }
        /// <summary>
        /// If provider version is set the verification results will be published to the Pact Broker
        /// </summary>
        public string ProviderVersion { get; set; }
        /// <summary>
        /// The Bearer token required to authenticate with the Pact Broker 
        /// </summary>
        public string PactBrokerToken { get; set; }
        /// <summary>
        /// The Pact Broker uri
        /// </summary>
        public string PactBrokerUri { get; set; }
        /// <summary>
        /// Web hos builder defaulted to <c>Microsoft.AspNetCore.WebHost.CreateDefaultBuilder</c>
        /// </summary>
        public Func<IWebHostBuilder> GetBaseWebHostBuilder { get; set; } = Microsoft.AspNetCore.WebHost.CreateDefaultBuilder;
        /// <summary>
        /// The type of the startup class to use when hosting the web application
        /// </summary>
        public Type AspNetCoreStartup { get; set; }        
    }
}
