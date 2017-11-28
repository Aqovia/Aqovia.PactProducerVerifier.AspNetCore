using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Aqovia.PactProducerVerifier.Sample
{
    public class TestStateProvider : BaseProviderStateMiddleware
    {
        public TestStateProvider(RequestDelegate next) : base(next)
        {
        }

        protected override IDictionary<string, Action> ProviderStates { get; }
    }
}
