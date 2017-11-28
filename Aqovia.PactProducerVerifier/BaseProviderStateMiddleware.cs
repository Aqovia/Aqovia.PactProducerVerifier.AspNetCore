using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Aqovia.PactProducerVerifier
{
    public abstract class BaseProviderStateMiddleware
    {
        private readonly RequestDelegate _next;
        
        protected BaseProviderStateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        protected abstract IDictionary<string, Action> ProviderStates { get; }

        public Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value == "/provider-states")
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                if (context.Request.Method == HttpMethod.Post.ToString() && context.Request.Body != null)
                {
                    string jsonRequestBody;
                    using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                    {
                        jsonRequestBody = reader.ReadToEnd();
                    }

                    var providerState = JsonConvert.DeserializeObject<ProviderState>(jsonRequestBody);

                    //A null or empty provider state key must be handled
                    if (!string.IsNullOrEmpty(providerState?.State))
                    {
                        ProviderStates[providerState.State].Invoke();
                    }

                    context.Response.WriteAsync(string.Empty);
                    return Task.CompletedTask;
                }
            }

            return this._next(context);
        }
   
    }

    public class ProviderState
    {
        public string State { get; set; }
        public string Consumer { get; set; }
    }
}