using EFCoreAspNetCore.Blogs;
using EFCoreAspNetCore.Blogs.Posts;
using EFCoreAspNetCore.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreAspNetCore.Tests.Blogs
{
    [TestClass]
    public class PostGetCommandTests : TestBase
    {
        [TestMethod]
        public void NotFound()
        {
            Assert.ThrowsException<ItemNotFoundException>(() => new PostQueryCommand(dbContext).Execute(1, 1111111));
        }

        [TestMethod]
        public void Execute_ById()
        {
            var results = new PostQueryCommand(dbContext).Execute(1, 1);

            //Full map is tested in Search command
            Assert.AreEqual(1, results.Id);
        }

        [TestMethod]
        public void Query_FiltersAndMaps()
        {
            var results = new PostQueryCommand(dbContext).Execute(1, (query) => (query as IQueryable<Data.Post>).OrderByDescending(p => p.PostId));

            Assert.AreEqual(2, results.Count);

            var item = results.First();
            Assert.AreEqual(2, item.Id);
            Assert.AreEqual("Seed Blog", item.Blog.Name);
            Assert.AreEqual("Second Post", item.Title);
            Assert.AreEqual(new DateTimeOffset(new DateTime(2018, 7, 1)), item.Created);
            Assert.AreEqual("This is the second post!", item.Content);
            Assert.AreEqual(null, item.UpdatedAsOfDate);
            Assert.AreEqual(null, item.UpdatedPost);

            item = results.Last();
            Assert.AreEqual(new DateTimeOffset(new DateTime(2018, 10, 1)), item.UpdatedAsOfDate);
            Assert.AreEqual(2, item.UpdatedPost.Id);

        }
    }
}
