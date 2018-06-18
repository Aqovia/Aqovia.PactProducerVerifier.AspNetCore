using System;
using Microsoft.AspNetCore.Hosting;

namespace Aqovia.PactProducerVerifier.AspNetCore
{
    public class ProducerVerifierConfiguration
    {
        public string TeamCityProjectName { get; set; }        
        public string PactBrokerUsername { get; set; }
        public string PactBrokerPassword { get; set; }
        public string PactBrokerUri { get; set; }
        public Func<IWebHostBuilder> GetBaseWebHostBuilder { get; set; } = Microsoft.AspNetCore.WebHost.CreateDefaultBuilder;
        public Type AspNetCoreStartup { get; set; }        
    }
}
