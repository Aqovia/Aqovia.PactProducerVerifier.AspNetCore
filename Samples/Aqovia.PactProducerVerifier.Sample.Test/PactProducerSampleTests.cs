using System;
using System.IO;
using System.Threading.Tasks;
using Aqovia.PactProducerVerifier.Api;
using Aqovia.PactProducerVerifier.AspNetCore;
using Microsoft.AspNetCore.Builder;

namespace Aqovia.PactProducerVerifier.Sample.Test
{        
    public class PactProducerSampleTests
    {
        private readonly PactProducerTests _pactProducerTests;
        private const int TeamCityMaxBranchLength = 19;
        public PactProducerSampleTests()
        {
            var configuration= new ProducerVerifierConfiguration
            {
                TeamCityProjectName = "PactProducerSampleTests",
                PactBrokerUri = "http://localhost:13800",
                AspNetCoreStartup = typeof(Startup),
                StartupAssemblyLocation = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "..\\..\\..\\..\\..\\Aqovia.PactProducerVerifier.Api")

            };
            _pactProducerTests = new PactProducerTests(configuration,  Console.WriteLine, ThisAssembly.Git.Branch, builder =>
            {
                builder.UseMiddleware(typeof(TestStateProvider));

            }, TeamCityMaxBranchLength);
        }
        
        public async Task EnsureApiHonoursPactWithConsumers()
        {
            using (var server = new TestPactBrokerServer(13800))
            {
                server.Start();
                await _pactProducerTests.EnsureApiHonoursPactWithConsumersAsync();
            }
        }
    }    

}
