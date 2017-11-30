using System;
using System.Threading.Tasks;
using uhttpsharp;

namespace Aqovia.PactProducerVerifier.Sample
{
    internal class InlineHandler : IHttpRequestHandler
    {
        private readonly Func<IHttpContext, Task> _handler;

        public InlineHandler(Func<IHttpContext,Task> handler)
        {
            _handler = handler;
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            return _handler(context);
        }
    }
}