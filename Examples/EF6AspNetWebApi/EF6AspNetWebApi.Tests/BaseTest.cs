using EF6AspNetWebApi.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Tests
{
    public class BaseTest : IDisposable
    {
        protected readonly ExampleContext dbContext;

        static BaseTest()
        {
            using (var context = new ExampleContext())
            {
                ExampleContextMetadata.Build(context);
            }
        }

        public BaseTest()
        {
            dbContext = new ExampleContext();
        }

        public void DatesAreSimilar(DateTimeOffset expected, DateTimeOffset actual)
        {
            Assert.IsTrue(Math.Abs(expected.Subtract(actual).Seconds) < 5);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }
}
