using System;
using System.Threading.Tasks;

namespace Aqovia.PactProducerVerifier.Sample.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new PactProducerSampleTests();
            var task = test.EnsureApiHonoursPactWithConsumers();
            task.Wait();
            Console.ReadLine();
        }
    }
}
