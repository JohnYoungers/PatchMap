using EFCoreWebApi.Blogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreWebApi.Tests.Blogs
{
    [TestClass]
    public class BlogSearchCommandTests : BaseTest
    {
        [TestMethod]
        public void FiltersAndMaps()
        {
            var results = new BlogSearchCommand(dbContext).Execute((query) => (query as IQueryable<Data.Blog>).Where(b => b.BlogId == 1));

            Assert.AreEqual(1, results.Count);

            var item = results.First();
            Assert.AreEqual(1, item.Id);
            Assert.AreEqual("Seed Blog", item.Name);
            Assert.AreEqual("http://sample.com", item.Url);
            CollectionAssert.AreEqual(new[] { "Tag 1", "Tag 2" }, item.Tags);

            Assert.AreEqual(1, item.Posts.Count);
            var post = item.Posts.First();
            Assert.AreEqual("First Post", post.Title);
        }
    }
}
