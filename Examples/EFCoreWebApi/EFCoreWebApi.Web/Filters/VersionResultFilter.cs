using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Reflection;

namespace EFCoreWebApi.Web.Filters
{
    public class VersionResultFilter : IResultFilter
    {
        public string Version { get; set; }

        public VersionResultFilter()
        {
            var assembly = Assembly.GetAssembly(typeof(BaseCommand));
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Version = fileVersionInfo.ProductVersion;
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add("X-API-version", Version);
        }

        public void OnResultExecuted(ResultExecutedContext context) { }
    }
}
