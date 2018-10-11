using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EFCoreAspNetCore.Web.Filters
{
    public class ODataResultFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            var odataDetails = context.HttpContext.Request.ODataFeature();
            if (odataDetails != null && odataDetails.TotalCount.HasValue)
            {
                context.HttpContext.Response.Headers.Add("X-ODATA-count", odataDetails.TotalCount.ToString());
            }
        }

        public void OnResultExecuted(ResultExecutedContext context) { }
    }
}
