using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace EF6AspNetWebApi.Web.Handlers
{
    public class ExposeHeadersHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            response.Headers.Add("Access-Control-Expose-Headers", "Content-Type, Content-Disposition, X-ODATA-count, X-API-version");
            return response;
        }
    }
}