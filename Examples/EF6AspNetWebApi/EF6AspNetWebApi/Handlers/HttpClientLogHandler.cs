using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Handlers
{
    public class HttpClientLogHandler : DelegatingHandler
    {
        public HttpClientLogHandler(HttpMessageHandler innerHandler)
            : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var logger = Application.LoggerFactory.CreateLogger<HttpClientLogHandler>();
            if (logger != null)
            {
                string content = null;
                if (request.Content != null)
                {
                    content = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                var context = new[]
                {
                    new KeyValuePair<string, object>("URL", request.RequestUri.ToString()),
                    new KeyValuePair<string, object>("HttpData", request.ToString()),
                    new KeyValuePair<string, object>("Body", content)
                };
                using (logger.BeginScope(context))
                {
                    logger.LogDebug("HTTP Message sent to {URL}");
                    logger.LogTrace("Message: " + Environment.NewLine + "{HttpData}" + Environment.NewLine + "{Body}");
                }
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            stopWatch.Stop();

            if (logger != null)
            {
                string content = null;
                if (response.Content != null)
                {
                    content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                var context = new[]
                {
                    new KeyValuePair<string, object>("URL", request.RequestUri.ToString()),
                    new KeyValuePair<string, object>("HttpData", response.ToString()),
                    new KeyValuePair<string, object>("Body", content),
                    new KeyValuePair<string, object>("ElapsedMilliseconds", stopWatch.ElapsedMilliseconds)
                };
                using (logger.BeginScope(context))
                {
                    logger.LogDebug("HTTP Message received in {ElapsedMilliseconds}ms from {URL}");
                    logger.LogTrace("Message: " + Environment.NewLine + "{HttpData}" + Environment.NewLine + "{Body}");
                }
            }

            return response;
        }
    }
}
