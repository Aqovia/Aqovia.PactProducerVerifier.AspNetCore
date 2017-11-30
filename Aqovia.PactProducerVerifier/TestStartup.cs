using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aqovia.PactProducerVerifier.AspNetCore
{
    public class TestStartup : IStartup
    {
        private readonly Type _starType;
        private readonly Action<IApplicationBuilder> _onWebAppStarting;
        private readonly object _apiStartup;

        public TestStartup(Type starType, string startupAssemblyLocation, Action<IApplicationBuilder> onWebAppStarting = null)
        {
            _starType = starType;
            _onWebAppStarting = onWebAppStarting;

            var builder = new ConfigurationBuilder()
                .SetBasePath(startupAssemblyLocation)
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
            var baseMethod = _starType.GetMethod(nameof(Configure));
            var methodParams = new List<Object>();
            methodParams.Add(app);

            foreach (ParameterInfo p in baseMethod.GetParameters())
            {
                if(p.ParameterType!= typeof(IApplicationBuilder))
                    methodParams.Add(app.ApplicationServices.GetService(p.ParameterType));                
            }            
            
            baseMethod.Invoke(_apiStartup, methodParams.ToArray());
            _onWebAppStarting?.Invoke(app);
        }
    }
}
