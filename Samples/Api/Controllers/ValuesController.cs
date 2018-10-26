using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Aqovia.PactProducerVerifier.Api.Controllers
{
    public class ValuesController : Controller
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
