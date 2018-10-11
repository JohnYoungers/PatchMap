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
            services.AddScoped((sp) => new ExampleContext());

            var serilog = new LoggerConfiguration().ReadFrom.AppSettings();
            Log.Logger = serilog.CreateLogger();
            services.AddLogging(builder => builder.AddSerilog(dispose: true));

            ServiceProvider = services.BuildServiceProvider();
            LoggerFactory = ServiceProvider.GetService<ILoggerFactory>();

            using (var context = GetDbContext())
            {
                ExampleContextMetadata.Build(context);
            }
        }

        public static ExampleContext GetDbContext()
        {
            var dbContext = new ExampleContext();
            var logger = LoggerFactory.CreateLogger<ExampleContext>();
            dbContext.Database.Log = (s => logger.LogTrace(s));

            return dbContext;
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
