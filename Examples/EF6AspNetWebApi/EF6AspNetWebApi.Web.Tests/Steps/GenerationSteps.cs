using EF6AspNetWebApi.Tests;
using EF6AspNetWebApi.Web.Tests.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace EF6AspNetWebApi.Web.Tests.Steps
{
    [Binding]
    public class GenerationSteps
    {
        private readonly AppFeatureContext context;

        public GenerationSteps(AppFeatureContext context)
        {
            this.context = context;
        }

        [When(@"I generated a string for placeholder S1")]
        public void GenerateString1()
        {
            context.AddPlaceholderValue("S1", Generator.Id<string>());
        }

        [When(@"I generated a string for placeholder S2")]
        public void GenerateString2()
        {
            context.AddPlaceholderValue("S2", Generator.Id<string>());
        }
    }
}
