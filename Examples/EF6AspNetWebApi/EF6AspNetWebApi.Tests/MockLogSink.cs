using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Tests
{
    /// <summary>
    /// Used to verify any logging a command may make
    /// </summary>
    public class MockLogSink : ILogEventSink
    {
        public static List<(string message, LogEvent ev)> LogEntries { get; set; }

        private readonly IFormatProvider _formatProvider;

        public MockLogSink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
            LogEntries = new List<(string, LogEvent)>();
        }

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage(_formatProvider);

            LogEntries.Add((message, logEvent));
        }
    }

    public static class MockLogSinkExtensions
    {
        public static LoggerConfiguration MockLogSink(this LoggerSinkConfiguration loggerConfiguration, IFormatProvider formatProvider = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            return loggerConfiguration.Sink(new MockLogSink(formatProvider), restrictedToMinimumLevel);
        }
    }
}
