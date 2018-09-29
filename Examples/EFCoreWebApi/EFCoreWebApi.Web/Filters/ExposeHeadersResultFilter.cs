using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Reflection;

namespace EFCoreWebApi.Web.Filters
{
    public class ExposeHeadersResultFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Type, Content-Disposition, X-ODATA-count, X-API-version");
        }

        public void OnResultExecuted(ResultExecutedContext context) { }
    }
}
