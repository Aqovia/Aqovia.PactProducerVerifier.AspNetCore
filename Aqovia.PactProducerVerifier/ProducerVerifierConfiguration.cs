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
        /// If provider version is set the verifiaction results will be publishe to the Pact Broker
        /// </summary>
        public string ProviderVersion { get; set; }
        /// <summary>
        /// Username required to authenticate with the Pact Broker 
        /// </summary>
        public string PactBrokerUsername { get; set; }
        /// <summary>
        /// Password required to authenticate with the Pact Broker
        /// </summary>
        public string PactBrokerPassword { get; set; }
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
