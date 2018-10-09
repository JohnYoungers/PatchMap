using EF6AspNetWebApi.Blogs;
using EF6AspNetWebApi.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Tests.Blogs
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
