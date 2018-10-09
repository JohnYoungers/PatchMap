using EFCoreAspNetCore.Blogs;
using EFCoreAspNetCore.Blogs.Posts;
using EFCoreAspNetCore.Exceptions;
using LinqKit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreAspNetCore.Tests.Blogs
{
    [TestClass]
    public class PostPatchCommandTests : BaseTest
    {
        [TestMethod]
        public void Validates()
        {
            var dbBlog = dbContext.Blogs.Add(SampleData.Blogs.Generic());
            dbBlog.Entity.Posts.Add(new Data.Post { Title = "A", Content = "B", DateCreated = DateTimeOffset.Now });
            dbContext.SaveChanges();

            var post = new PostViewModel { };
            var results = new PostPatchCommand(dbContext).Execute(dbBlog.Entity.BlogId, null, post.ToPatchOperations());
            results.AssertHasValidationResult("Title", "Title is required");
            results.AssertHasValidationResult("Content", "Content is required");

            post.Title = "".PadRight(151, 'A');
            results = new PostPatchCommand(dbContext).Execute(dbBlog.Entity.BlogId, null, post.ToPatchOperations());
            results.AssertHasValidationResult("Title", "Title length must be equal to or less than 150 characters");

            post.UpdatedAsOfDate = DateTimeOffset.Now;
            results = new PostPatchCommand(dbContext).Execute(dbBlog.Entity.BlogId, null, post.ToPatchOperations());
            results.AssertHasValidationResult("UpdatedPost", "Updated Post requires both a Post and an As Of date");

            post.UpdatedAsOfDate = null;
            post.UpdatedPost = PostSummaryViewModel.Map().Invoke(dbBlog.Entity.Posts.First());
            results = new PostPatchCommand(dbContext).Execute(dbBlog.Entity.BlogId, null, post.ToPatchOperations());
            results.AssertHasValidationResult("UpdatedPost", "Updated Post requires both a Post and an As Of date");
        }

        [TestMethod]
        public void Inserts()
        {
            var dbBlog = dbContext.Blogs.Add(SampleData.Blogs.Generic());
            dbContext.SaveChanges();
            var nextId = dbContext.Posts.Max(p => p.PostId) + 1;
            var post = new PostViewModel
            {
                Title = $"Post " + Generator.Id<string>(),
                Content = $"Great content {Generator.Id<string>()}"
            };
            var results = new PostPatchCommand(dbContext).Execute(dbBlog.Entity.BlogId, null, post.ToPatchOperations());

            Assert.IsTrue(results.Succeeded);
            Assert.AreEqual(true, results.IsNew);
            Assert.AreEqual(nextId.ToString(), results.EntityId);

            var refreshedPost = new PostGetCommand(dbContext).Execute(dbBlog.Entity.BlogId, results.Entity.Id);
            Assert.AreEqual(post.Title, refreshedPost.Title);
            Assert.AreEqual(post.Content, refreshedPost.Content);
            DatesAreSimilar(DateTimeOffset.Now, refreshedPost.Created);
            Assert.AreEqual(null, refreshedPost.UpdatedAsOfDate);
            Assert.AreEqual(null, refreshedPost.UpdatedPost);
        }

        [TestMethod]
        public void Update_NotFound()
        {
            Assert.ThrowsException<ItemNotFoundException>(() => new PostPatchCommand(dbContext).Execute(1, 12345, new PostViewModel().ToPatchOperations()));
        }

        [TestMethod]
        public void Updates()
        {
            var dbBlog = dbContext.Blogs.Add(SampleData.Blogs.Generic());
            dbBlog.Entity.Posts.Add(new Data.Post { Title = "A", Content = "B", DateCreated = DateTimeOffset.Now });
            dbBlog.Entity.Posts.Add(new Data.Post { Title = "C", Content = "D", DateCreated = DateTimeOffset.Now });
            dbContext.SaveChanges();

            var dbPost = dbBlog.Entity.Posts.First();
            var dbPost2 = dbBlog.Entity.Posts.Skip(1).First();
            var post = new PostGetCommand(dbContext).Execute(dbBlog.Entity.BlogId, dbPost.PostId);
            post.Title = post.Title + "Updated";
            post.Content = post.Content + "Updated";
            post.UpdatedAsOfDate = DateTimeOffset.Now;
            post.UpdatedPost = new PostSummaryViewModel { Id = dbPost2.PostId };

            var results = new PostPatchCommand(dbContext).Execute(dbBlog.Entity.BlogId, dbPost.PostId, post.ToPatchOperations());

            Assert.IsTrue(results.Succeeded);
            Assert.AreEqual(false, results.IsNew);

            var refreshedPost = new PostGetCommand(dbContext).Execute(dbBlog.Entity.BlogId, dbPost.PostId);
            //These fields can only be set on insert
            Assert.AreEqual("A", refreshedPost.Title);
            Assert.AreEqual("B", refreshedPost.Content);

            //Verifying remaining map
            DatesAreSimilar(DateTimeOffset.Now, refreshedPost.UpdatedAsOfDate.Value);
            Assert.AreEqual(dbPost2.PostId, refreshedPost.UpdatedPost.Id);
        }
    }
}
