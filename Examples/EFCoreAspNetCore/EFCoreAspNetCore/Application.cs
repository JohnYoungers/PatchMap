using EFCoreAspNetCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreAspNetCore
{
    public static class Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static ILoggerFactory LoggerFactory { get; private set; }

        public static void InitializeServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var serilog = new LoggerConfiguration().ReadFrom.Configuration(configuration);
            Log.Logger = serilog.CreateLogger();
            serviceCollection.AddLogging(builder => builder.AddSerilog(dispose: true));

            serviceCollection.AddDbContext<ExampleContext>(options => options.UseSqlServer(configuration.GetConnectionString("EFCore")));
        }
        public static void FinalizeInitialization(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            LoggerFactory = ServiceProvider.GetService<ILoggerFactory>();

            using (var serviceScope = ServiceProvider.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ExampleContext>();
                context.InitializeDatabase();
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
