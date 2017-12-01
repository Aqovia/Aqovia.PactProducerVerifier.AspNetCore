using System.IO;
using System.Threading.Tasks;
using Aqovia.PactProducerVerifier.Api;
using Aqovia.PactProducerVerifier.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Xunit;
using Xunit.Abstractions;

namespace Aqovia.PactProducerVerifier.Sample.Test
{    
    public class PactProducerSampleTestsXUnit
    {
        private readonly PactProducerTests _pactProducerTests;
        private const int TeamCityMaxBranchLength = 19;
        public PactProducerSampleTestsXUnit(ITestOutputHelper output)
        {
            var configuration = new ProducerVerifierConfiguration
            {
                TeamCityProjectName = "PactProducerSampleTests",
                PactBrokerUri = "http://localhost:13800",
                AspNetCoreStartup = typeof(Startup),                
                StartupAssemblyLocation = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\..\\Api"),                
            };
            _pactProducerTests = new PactProducerTests(configuration, output.WriteLine, "test-branch", builder =>
            {
                builder.UseMiddleware(typeof(TestStateProvider));

            }, TeamCityMaxBranchLength);
        }

        [Fact]                
        public async Task EnsureApiHonoursPactWithConsumers()
        {
            using (var server = new TestPactBrokerServer(13800, "test-branch"))
            {
                server.Start();
                await _pactProducerTests.EnsureApiHonoursPactWithConsumersAsync();
            }            
        }
    }



}
