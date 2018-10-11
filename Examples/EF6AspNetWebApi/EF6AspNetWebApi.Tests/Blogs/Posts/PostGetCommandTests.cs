using EF6AspNetWebApi.Blogs.Posts;
using EF6AspNetWebApi.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Tests.Blogs.Posts
{
    [TestClass]
    public class PostGetCommandTests : BaseTest
    {
        [TestMethod]
        public void NotFound()
        {
            Assert.ThrowsException<ItemNotFoundException>(() => new PostGetCommand(dbContext).Execute(1, 1111111));
        }

        [TestMethod]
        public void Execute()
        {
            var results = new PostGetCommand(dbContext).Execute(1, 1);

            //Full map is tested in Search command
            Assert.AreEqual(1, results.Id);
        }
    }
}
