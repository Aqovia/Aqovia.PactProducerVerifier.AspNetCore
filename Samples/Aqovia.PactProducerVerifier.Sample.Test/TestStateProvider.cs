using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Aqovia.PactProducerVerifier.Sample.Test
{
    public class TestStateProvider : BaseProviderStateMiddleware
    {
        public TestStateProvider(RequestDelegate next) : base(next)
        {
        }

        protected override IDictionary<string, Action> ProviderStates { get; }
    }
}
