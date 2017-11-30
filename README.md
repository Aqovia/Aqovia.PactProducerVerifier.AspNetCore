# Aqovia.PactProducerVerifier

A utility for verifying producer code against all consumers on the Pact Broker.
It calls the pact broker and retrieves all pacts where it is a producer (using TeamCityProjectName config setting)
and allows for branching using either the passed in Git Branch Name or the teamcity environment variable "ComponentBranch"

[![Build status](https://ci.appveyor.com/api/projects/status/jltbacetwhyu9t2x/branch/master?svg=true)](https://ci.appveyor.com/project/aqovia/aqovia-pactproducerverifier-aspnetcore/branch/master)
[![NuGet Badge](https://buildstats.info/nuget/Aqovia.PactProducerVerifier.AspNetCore)](https://www.nuget.org/packages/Aqovia.PactProducerVerifier.AspNetCore)/)

## Assumptions

Build server is TeamCity, as it uses the environment variable "ComponentBranch" to determine the branch of the code on the build server

## Getting started

This uses the beta version of PactNet, and Team City.

* Install the latest beta version 2.0.X-beta of PactNet package (use allow Pre-release option)
* Install this package Aqovia.PactProducerVerifier.AspNetCore
* Install a test framework such as XUnit
* Add `<RuntimeIdentifier>win7-x86</RuntimeIdentifier>` to the csproj file. [see](https://github.com/dotnet/sdk/issues/909)
* Install GitInfo if you require to work out the git branch name locally 

* Add the test (example using XUnit)
```
    public class PactProducerTests
    {
        private readonly Aqovia.PactProducerVerifier.PactProducerTests _pactProducerTests;
        private const int TeamCityMaxBranchLength = 19;
        public PactProducerTests(ITestOutputHelper output)
        {            
			var configuration = new ProducerVerifierConfiguration
            {
                TeamCityProjectName = "<YOUR NAME OF THE PROJECT (PRODUCER)",
                PactBrokerUri = "<YOUR PACT BROKER URL>",
				PactBrokerUsername = <YOUR PACT BROKER USERNAME OR NULL> ,
                PactBrokerPassword = <YOUR PACT BROKER PASSWORD OR NULL>,                
                AspNetCoreStartup = typeof(Startup),
                StartupAssemblyLocation = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\..\\Aqovia.PactProducerVerifier.Api")
            };

			 _pactProducerTests = new PactProducerTests(configuration, output.WriteLine, ThisAssembly.Git.Branch, builder =>
            {
                builder.UseMiddleware(typeof(TestStateProvider));

            }, TeamCityMaxBranchLength);
        }


        [Fact]
        public void EnsureApiHonoursPactWithConsumers()
        {
           _pactProducerTests.EnsureApiHonoursPactWithConsumers();
        }
    }
```
The PactProducerTests constructor takes in 5 parameters:
* Configuration settings
* An Action<string> - this is used so the output of the pact test is outputted to the test results (in XUnit in this example)
* The branch this code is in. This is used locally, but if running on the build server it uses the environment variable "ComponentBranch"
* Callback for installing middleware in the AspNET pipeline. i.e. a custom state provider
* The maximum branch name length (optional)

## Sample
A sample is included in the source - in the samples folder. To use this:
* Update the PactBrokerUri configuration setting to the uri of the broker your using.
