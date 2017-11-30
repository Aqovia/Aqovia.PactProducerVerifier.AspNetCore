using System;

namespace Aqovia.PactProducerVerifier.AspNetCore
{
    public class ProducerVerifierConfiguration
    {
        public string TeamCityProjectName { get; set; }        
        public string PactBrokerUsername { get; set; }
        public string PactBrokerPassword { get; set; }
        public string PactBrokerUri { get; set; }
        public Type AspNetCoreStartup { get; set; }
        public string StartupAssemblyLocation { get; set; }
    }
}
