using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace EF6AspNetWebApi.Web.Handlers
{
    public class VersionHandler : DelegatingHandler
    {
        public string Version { get; set; }

        public VersionHandler()
        {
            var assembly = Assembly.GetAssembly(typeof(BaseCommand));
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Version = fileVersionInfo.ProductVersion;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            response.Headers.Add("X-API-version", Version);
            return response;
        }
    }
}