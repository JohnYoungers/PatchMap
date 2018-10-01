using EFCoreWebApi.Blogs;
using EFCoreWebApi.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreWebApi.Tests.Blogs
{
    [TestClass]
    public class BlogGetCommandTests : BaseTest
    {
        [TestMethod]
        public void NotFound()
        {
            Assert.ThrowsException<ItemNotFoundException>(() => new BlogGetCommand(dbContext).Execute(1111111));
        }

        [TestMethod]
        public void Execute()
        {
            var results = new BlogGetCommand(dbContext).Execute(1);

            //Full map is tested in Search command
            Assert.AreEqual("Seed Blog", results.Name);
        }
    }
}
