using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EF6AspNetWebApi.Web.Tests.Contexts
{
    public class HttpContext
    {
        public string ResponseText { get; private set; }
        public HttpResponseMessage Response { get; private set; }

        public void SetResponse(HttpResponseMessage response)
        {
            Response = response;
            ResponseText = response.Content.ReadAsStringAsync().Result;
        }

        public JToken ResponseAsJson()
        {
            return string.IsNullOrEmpty(ResponseText) ? null : JToken.Parse(ResponseText);
        }

        public XDocument ResponseAsXml()
        {
            return string.IsNullOrEmpty(ResponseText) ? null : XDocument.Parse(ResponseText);
        }
    }
}
