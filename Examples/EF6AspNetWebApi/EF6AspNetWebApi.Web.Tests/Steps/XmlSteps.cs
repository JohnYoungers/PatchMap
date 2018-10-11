using EF6AspNetWebApi.Web.Tests.Contexts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using TechTalk.SpecFlow;

namespace EF6AspNetWebApi.Web.Tests.Steps
{
    [Binding]
    class XmlSteps
    {
        private readonly AppFeatureContext context;
        private readonly HttpContext httpContext;

        public XmlSteps(AppFeatureContext context, HttpContext httpContext)
        {
            this.context = context;
            this.httpContext = httpContext;
        }

        [Then(@"the XML value at '(.*)' should be '(.*)' in the namespace '(.*)'")]
        public void ThenTheXmlAtShouldBe(string selector, string expected, string xmlNamespace)
        {
            var expectedWithSubstitutions = PopulatePlaceholders(expected);
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("ns", xmlNamespace);

            var actual = httpContext.ResponseAsXml().XPathSelectElement(selector, xnm)?.Value;
            Assert.AreEqual(expectedWithSubstitutions, actual.ToString(), string.Format("Does not equal expected value at {0}", selector));
        }

        private string PopulatePlaceholders(string value)
        {
            return context.PopulatePlaceholders(value);
        }
    }
}
