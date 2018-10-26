using System.Net;
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
        private const int _port = 13800;
        public PactProducerSampleTestsXUnit(ITestOutputHelper output)
        {
            var configuration = new ProducerVerifierConfiguration
            {
                TeamCityProjectName = "PactProducerSampleTests",
                PactBrokerUri = $"http://{IPAddress.Loopback.ToString()}:{_port}",
                AspNetCoreStartup = typeof(Startup)
            };
            _pactProducerTests = new PactProducerTests(configuration, output.WriteLine, "test-branch", builder =>
            {
                builder.UseMiddleware(typeof(TestStateProvider));
            }, TeamCityMaxBranchLength);
        }

        [Fact]                
        public async Task EnsureApiHonoursPactWithConsumers()
        {
            using (var server = new TestPactBrokerServer(_port, "test-branch"))
            {
                server.Start();
                await _pactProducerTests.EnsureApiHonoursPactWithConsumersAsync();
            }            
        }
    }
}
