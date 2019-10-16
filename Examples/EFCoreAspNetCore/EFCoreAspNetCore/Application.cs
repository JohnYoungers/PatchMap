using EFCoreAspNetCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        public static void AddServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddDbContext<ExampleContext>(options => options.UseSqlServer(configuration.GetConnectionString("EFCore")));
        }
        public static void Configure(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            LoggerFactory = ServiceProvider.GetService<ILoggerFactory>();

            using (var serviceScope = ServiceProvider.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ExampleContext>();
                context.InitializeDatabase();
            }
        }

        public static void LogInformation<T>(string message, params (string key, object value)[] context) => LogMessage<T>((logger) => logger.LogInformation(message), context);
        public static void LogDebug<T>(string message, params (string key, object value)[] context) => LogMessage<T>((logger) => logger.LogDebug(message), context);
        public static void LogError<T>(string message, params (string key, object value)[] context) => LogException<T>(null, message, context);
        public static void LogException<T>(Exception ex, string message, params (string key, object value)[] context) => LogMessage<T>((logger) => logger.LogError(ex, message), context);
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
