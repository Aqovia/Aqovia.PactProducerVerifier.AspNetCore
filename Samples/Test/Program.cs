using System;

namespace Aqovia.PactProducerVerifier.Sample.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var test = new PactProducerSampleTests();
            var task = test.EnsureApiHonoursPactWithConsumers();
            task.Wait();
            Console.ReadLine();
        }
    }
}
