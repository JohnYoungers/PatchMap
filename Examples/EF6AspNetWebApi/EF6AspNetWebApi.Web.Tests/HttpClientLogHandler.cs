using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Web.Tests
{
    public class HttpClientLogHandler : DelegatingHandler
    {
        public HttpClientLogHandler(HttpMessageHandler innerHandler)
            : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string content = null;
            if (request.Content != null)
            {
                content = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            Debug.WriteLine("HTTP Message sent to " + request.RequestUri.ToString());
            Debug.WriteLine("Message: " + Environment.NewLine + request.ToString() + Environment.NewLine + content);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            stopWatch.Stop();

            content = null;
            if (response.Content != null)
            {
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            Debug.WriteLine("HTTP Message received in " + stopWatch.ElapsedMilliseconds + "ms from " + request.RequestUri.ToString());
            Debug.WriteLine("Message: " + Environment.NewLine + response.ToString() + Environment.NewLine + content);

            return response;
        }
    }
}
