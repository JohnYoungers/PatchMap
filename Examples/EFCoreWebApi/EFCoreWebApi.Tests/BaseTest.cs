using EFCoreWebApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EFCoreWebApi.Tests
{
    public class BaseTest : IDisposable
    {
        public static readonly LoggerFactory SqlLogger = new LoggerFactory(new[] { new DebugLoggerProvider((cat, level) => cat == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information) });
        private static ServiceProvider ServiceProvider = new ServiceCollection()
                    .AddDbContext<ExampleContext>(o => o.UseLoggerFactory(SqlLogger).UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=example;Trusted_Connection=True;ConnectRetryCount=0"))
                    .BuildServiceProvider();

        static BaseTest()
        {
            using (var serviceScope = ServiceProvider.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ExampleContext>();
                context.InitializeDatabase();
            }
        }

        protected readonly IServiceScope scope;
        protected readonly ExampleContext dbContext;

        public BaseTest()
        {
            scope = ServiceProvider.GetService<IServiceScopeFactory>().CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<ExampleContext>();
        }

        public void Dispose()
        {
            scope.Dispose();
        }
    }
}
