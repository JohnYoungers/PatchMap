using EF6AspNetWebApi.Web.Tests.Contexts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace EF6AspNetWebApi.Web.Tests.Steps
{
    [Binding]
    public class HttpSteps
    {
        private readonly AppFeatureContext context;
        private readonly HttpContext httpContext;

        public HttpSteps(AppFeatureContext context, HttpContext httpContext)
        {
            this.context = context;
            this.httpContext = httpContext;
        }

        [When(@"I GET '(.*)'")]
        public void WhenWeGET(string url)
        {
            httpContext.SetResponse(GetHttpClient().GetAsync(Substitute(url)).Result);
        }

        [When(@"I GET '(.*)' requesting a XML response")]
        public void WhenWeGETXML(string url)
        {
            httpContext.SetResponse(GetHttpClient("application/xml").GetAsync(Substitute(url)).Result);
        }

        [When(@"I POST '(.*)' with the following:")]
        public void WhenWePOSTWithTheFollowing(string url, string bodyString)
        {
            var http = GetHttpClient();
            var messageContent = new StringContent(Substitute(bodyString), Encoding.UTF8, "application/json");

            httpContext.SetResponse(http.PostAsync(Substitute(url), messageContent).Result);
        }

        [When(@"I PUT '(.*)' with the following:")]
        public void WhenWePUTWithTheFollowing(string url, string bodyString)
        {
            var http = GetHttpClient();
            var messageContent = new StringContent(Substitute(bodyString), Encoding.UTF8, "application/json");

            httpContext.SetResponse(http.PutAsync(Substitute(url), messageContent).Result);
        }

        [When(@"I PATCH '(.*)' with the following:")]
        public void WhenWePATCHWithTheFollowing(string url, string bodyString)
        {
            var http = GetHttpClient();
            var messageContent = new StringContent(Substitute(bodyString), Encoding.UTF8, "application/json");

            httpContext.SetResponse(http.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), Substitute(url)) { Content = messageContent }).Result);
        }

        [When(@"I DELETE '(.*)'")]
        public void WhenIDELETE(string url)
        {
            httpContext.SetResponse(GetHttpClient().DeleteAsync(Substitute(url)).Result);
        }

        [Then(@"the header '(.*)' should be '(.*)'")]
        public void ThenTheHeaderShouldBe(string header, string value)
        {
            Assert.IsTrue(httpContext.Response.Headers.GetValues(header).Any(v => v == value));
        }

        [Then(@"the content type should be '(.*)'")]
        public void ThenTheContentTypeShouldBe(string value)
        {
            Assert.IsTrue(String.Equals(httpContext.Response.Content.Headers.ContentType.MediaType, value, StringComparison.CurrentCultureIgnoreCase));
        }

        [Then(@"the content disposition name should be '(.*)' and filename should be '(.*)'")]
        public void ThenTheContentDispositionShouldBe(string type, string fileName)
        {
            Assert.IsTrue(string.Equals(httpContext.Response.Content.Headers.ContentDisposition.DispositionType, Substitute(type), StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(string.Equals(httpContext.Response.Content.Headers.ContentDisposition.FileName, Substitute(fileName), StringComparison.CurrentCultureIgnoreCase));
        }

        [Then(@"the status should be (.*)")]
        public void ThenTheStatusShouldBe(int expected)
        {
            Assert.AreEqual(expected, (int)httpContext.Response.StatusCode, "Incorrect HTTP status code");
        }

        [Then(@"the response body should be empty")]
        public void ThenTheResponseBodyShouldBeEmpty()
        {
            var responseAsString = httpContext.ResponseText;
            Assert.IsTrue(string.IsNullOrEmpty(responseAsString), "Expected the response body to be empty, but was '{0}'", responseAsString);
        }

        [Then(@"the response body should contain '(.*)'")]
        public void ThenTheResponseBodyShouldContain(string contents)
        {
            Assert.IsTrue(httpContext.ResponseText.Contains(contents));
        }

        [Then(@"the response body should not contain '(.*)'")]
        public void ThenTheResponseBodyShouldNotContain(string contents)
        {
            Assert.IsTrue(!httpContext.ResponseText.Contains(contents));
        }

        private HttpClient GetHttpClient(string acceptType = null)
        {
            var service = new HttpClient(new HttpClientLogHandler(new HttpClientHandler() { UseDefaultCredentials = true }))
            {
                BaseAddress = new Uri(ConfigurationManager.AppSettings["WebsiteUrl"]),
                Timeout = new TimeSpan(0, 0, 60)
            };
            service.DefaultRequestHeaders.Add("ci-test", true.ToString());
            if (!string.IsNullOrEmpty(acceptType))
            {
                service.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));
            }

            return service;
        }

        private string Substitute(string value)
        {
            return context.PopulatePlaceholders(value);
        }
    }
}
