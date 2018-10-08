using EFCoreWebApi.Blogs;
using EFCoreWebApi.Blogs.Posts;
using EFCoreWebApi.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreWebApi.Tests.Blogs
{
    [TestClass]
    public class BlogPatchCommandTests : BaseTest
    {
        [TestMethod]
        public void Validates()
        {
            var blog = new BlogViewModel { };
            var results = new BlogPatchCommand(dbContext).Execute(null, blog.ToPatchOperations());
            results.AssertHasValidationResult("Name", "Name is required");
            results.AssertHasValidationResult("Url", "Url is required");

            blog.Name = "".PadRight(101, 'A');
            results = new BlogPatchCommand(dbContext).Execute(null, blog.ToPatchOperations());
            results.AssertHasValidationResult("Name", "Name length must be equal to or less than 100 characters");

            blog.Name = "Seed Blog";
            results = new BlogPatchCommand(dbContext).Execute(null, blog.ToPatchOperations());
            results.AssertHasValidationResult("Name", "Name of Seed Blog already exists");

            //Promoted Post is required on update but not on insert
            Assert.IsTrue(!results.ValidationResults.Any(vr => vr.MemberNames.Any(n => n == "PromotedPost")));

            results = new BlogPatchCommand(dbContext).Execute(1, blog.ToPatchOperations());
            results.AssertHasValidationResult("PromotedPost", "Promoted Post is required");
        }

        [TestMethod]
        public void Inserts()
        {
            var nextId = dbContext.Blogs.Max(b => b.BlogId) + 1;
            var blog = new BlogViewModel
            {
                Name = $"Blog " + Generator.Id<string>(),
                Url = $"Http://www.{Generator.Id<string>()}.org",
                Tags = new List<string> { "A", "B", "C" }
            };
            var results = new BlogPatchCommand(dbContext).Execute(null, blog.ToPatchOperations());

            Assert.IsTrue(results.Succeeded);
            Assert.AreEqual(true, results.IsNew);
            Assert.AreEqual(nextId.ToString(), results.EntityId);

            var refreshedBlog = new BlogGetCommand(dbContext).Execute(results.Entity.Id);
            Assert.AreEqual(blog.Name, refreshedBlog.Name);
            Assert.AreEqual(blog.Url, refreshedBlog.Url);
            CollectionAssert.AreEqual(new[] { "A", "B", "C" }, refreshedBlog.Tags);
        }

        [TestMethod]
        public void Update_NotFound()
        {
            Assert.ThrowsException<ItemNotFoundException>(() => new BlogPatchCommand(dbContext).Execute(111111, new BlogViewModel().ToPatchOperations()));
        }

        [TestMethod]
        public void Updates()
        {
            var dbBlog = dbContext.Blogs.Add(SampleData.Blogs.Generic());
            dbBlog.Entity.Posts.Add(new Data.Post { Title = "A", Content = "B", DateCreated = DateTimeOffset.Now });
            dbContext.SaveChanges();

            var blog = new BlogGetCommand(dbContext).Execute(dbBlog.Entity.BlogId);
            blog.Name = blog.Name + "Updated";
            blog.Tags = new List<string> { "A", "D" };
            blog.PromotedPost = new PostSummaryViewModel { Id = dbBlog.Entity.Posts[0].PostId };

            var results = new BlogPatchCommand(dbContext).Execute(dbBlog.Entity.BlogId, blog.ToPatchOperations());

            Assert.IsTrue(results.Succeeded);
            Assert.AreEqual(false, results.IsNew);

            var refreshedBlog = new BlogGetCommand(dbContext).Execute(results.Entity.Id);
            Assert.AreEqual(blog.Name, refreshedBlog.Name);
            CollectionAssert.AreEqual(new[] { "A", "D" }, refreshedBlog.Tags);
            Assert.AreEqual("A", refreshedBlog.PromotedPost.Title);
        }
    }
}
