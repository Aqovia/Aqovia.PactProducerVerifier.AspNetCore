using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aqovia.PactProducerVerifier
{
    public class TestStartup : IStartup
    {
        private readonly Type _starType;
        private readonly Action<IApplicationBuilder> _onWebAppStarting;
        private readonly object _apiStartup;

        public TestStartup(Type starType, Action<IApplicationBuilder> onWebAppStarting = null)
        {
            _starType = starType;
            _onWebAppStarting = onWebAppStarting;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(_starType.Assembly.Location))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();            

            
            _apiStartup = Activator.CreateInstance(starType, builder.Build());
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var baseMethod = _starType.GetMethod(nameof(ConfigureServices));
            baseMethod.Invoke(_apiStartup,new object[]{ services } );
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetService<IHostingEnvironment>();
            var baseMethod = _starType.GetMethod(nameof(Configure));
            baseMethod.Invoke(_apiStartup, new object[] { app, env });
            _onWebAppStarting?.Invoke(app);
        }
    }
}
