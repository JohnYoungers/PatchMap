using EFCoreAspNetCore.Blogs;
using EFCoreAspNetCore.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreAspNetCore.Tests.Blogs
{
    [TestClass]
    public class BlogGetCommandTests : TestBase
    {
        [TestMethod]
        public void NotFound()
        {
            Assert.ThrowsException<ItemNotFoundException>(() => new BlogQueryCommand(dbContext).Execute(1111111));
        }

        [TestMethod]
        public void Execute_ById()
        {
            var results = new BlogQueryCommand(dbContext).Execute(1);

            //Full map is tested in Search command
            Assert.AreEqual("Seed Blog", results.Name);
        }

        [TestMethod]
        public void Query_FiltersAndMaps()
        {
            var results = new BlogQueryCommand(dbContext).Execute((query) => (query as IQueryable<Data.Blog>).Where(b => b.BlogId == 1));

            Assert.AreEqual(1, results.Count);

            var item = results.First();
            Assert.AreEqual(1, item.Id);
            Assert.AreEqual("Seed Blog", item.Name);
            Assert.AreEqual("http://sample.com", item.Url);
            CollectionAssert.AreEqual(new[] { "Tag 1", "Tag 2" }, item.Tags);

            Assert.AreEqual(2, item.Posts.Count);
            var post = item.Posts.Last();
            Assert.AreEqual(1, post.Id);
            Assert.AreEqual("First Post", post.Title);

            Assert.AreEqual("First Post", item.PromotedPost.Title);
        }
    }
}
