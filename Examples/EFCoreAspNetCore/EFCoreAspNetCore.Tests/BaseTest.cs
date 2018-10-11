using EFCoreAspNetCore.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EFCoreAspNetCore.Tests
{
    public class BaseTest : IDisposable
    {
        static BaseTest()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var services = new ServiceCollection();

            Application.InitializeServices(services, configuration);
            Application.FinalizeInitialization(services.BuildServiceProvider());
        }

        protected readonly IServiceScope scope;
        protected readonly ExampleContext dbContext;

        public BaseTest()
        {
            scope = Application.ServiceProvider.GetService<IServiceScopeFactory>().CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<ExampleContext>();
        }

        public void DatesAreSimilar(DateTimeOffset expected, DateTimeOffset actual)
        {
            Assert.IsTrue(Math.Abs(expected.Subtract(actual).Seconds) < 5);
        }

        public void Dispose()
        {
            scope.Dispose();
        }
    }
}
