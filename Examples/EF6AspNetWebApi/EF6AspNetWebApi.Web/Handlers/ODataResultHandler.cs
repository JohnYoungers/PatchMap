using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.OData.Extensions;

namespace EF6AspNetWebApi.Web.Handlers
{
    public class ODataResultHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            var odataProperties = request.ODataProperties();
            if (odataProperties.TotalCount.HasValue)
            {
                response.Headers.Add("X-ODATA-count", odataProperties.TotalCount.ToString());
            }

            return response;
        }
    }
}