using EF6AspNetWebApi.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi
{
    public static class Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static ILoggerFactory LoggerFactory { get; private set; }

        public static void InitializeAndBuildProvider(IServiceCollection serviceCollection = null)
        {
            var services = serviceCollection ?? new ServiceCollection();

            var serilog = new LoggerConfiguration().ReadFrom.AppSettings();
            Log.Logger = serilog.CreateLogger();
            services.AddLogging(builder => builder.AddSerilog(dispose: true));

            services.AddScoped<ExampleContext>();

            ServiceProvider = services.BuildServiceProvider();
            LoggerFactory = ServiceProvider.GetService<ILoggerFactory>();

            using (var scope = ServiceProvider.CreateScope())
            {
                ExampleContextMetadata.Build(scope.ServiceProvider.GetRequiredService<ExampleContext>());
            }
        }

        public static void LogInformation<T>(string message, params (string key, object value)[] context) => LogMessage<T>((logger) => logger.LogInformation(message));
        public static void LogDebug<T>(string message, params (string key, object value)[] context) => LogMessage<T>((logger) => logger.LogDebug(message));
        public static void LogError<T>(string message, params (string key, object value)[] context) => LogException<T>(null, message, context);
        public static void LogException<T>(Exception ex, string message, params (string key, object value)[] context) => LogMessage<T>((logger) => logger.LogError(ex, message));
        private static void LogMessage<T>(Action<ILogger<T>> action, params (string key, object value)[] context)
        {
            var logger = LoggerFactory.CreateLogger<T>();
            using (logger.BeginScope(context?.Select(kvp => new KeyValuePair<string, object>(kvp.key, kvp.value))))
            {
                action(logger);
            }
        }
    }
}
