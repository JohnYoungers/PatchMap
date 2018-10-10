using EF6AspNetWebApi.Web.Tests.Contexts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace EF6AspNetWebApi.Web.Tests.Steps
{
    [Binding]
    class JsonSteps
    {
        private readonly AppFeatureContext context;
        private readonly HttpContext httpContext;

        public JsonSteps(AppFeatureContext context, HttpContext httpContext)
        {
            this.context = context;
            this.httpContext = httpContext;
        }

        [Then(@"the JSON at '(.*)' should be '(.*)'")]
        public void ThenTheJsonAtShouldBe(string selector, string expected)
        {
            var expectedWithSubstitutions = PopulatePlaceholders(expected);
            Assert.AreEqual(expectedWithSubstitutions, GetBySelectorToString(selector), string.Format("Does not equal expected value at {0}", selector));
        }

        [Then(@"the JSON at '(.*)' should contain '(.*)'")]
        public void ThenTheJsonAtShouldContain(string selector, string expected)
        {
            var expectedWithSubstitutions = PopulatePlaceholders(expected);
            Assert.IsTrue(GetBySelectorToString(selector).Contains(expectedWithSubstitutions), $"{selector} does not contain {expectedWithSubstitutions}");
        }

        private string GetBySelectorToString(string selector)
        {
            var jsonObject = httpContext.ResponseAsJson().SelectToken(selector, true);

            string actualAsString = null;
            switch (jsonObject.Type)
            {
                case JTokenType.Date:
                    actualAsString = jsonObject.ToObject<DateTime>().ToString("M/d/yyyy HH:mm:ss");
                    break;
                default:
                    actualAsString = jsonObject.ToString();
                    break;
            }

            return actualAsString;
        }

        [Then(@"the JSON at '(.*)' should not exist")]
        public void ThenTheJsonAtShouldNotExist(string selector)
        {
            Assert.ThrowsException<JsonException>(() => httpContext.ResponseAsJson().SelectToken(selector, true));
        }

        [Then(@"the JSON array should have a length of '(.*)'")]
        public void ThenTheJSONArrayShouldHaveALengthOf(int length)
        {
            Assert.AreEqual(httpContext.ResponseAsJson().ToObject<List<object>>().Count, length);
        }

        [Then(@"the response content starts with '(.*)'")]
        public void ThenTheResponseContentStartsWith(string text)
        {
            Assert.IsTrue(httpContext.ResponseText.StartsWith(PopulatePlaceholders(text)));
        }

        private string PopulatePlaceholders(string value)
        {
            return context.PopulatePlaceholders(value);
        }
    }
}
