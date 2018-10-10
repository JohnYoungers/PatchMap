using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Web.Tests
{
    [TestClass]
    public static class TestContainer
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext ctx)
        {
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
        }
    }
}
