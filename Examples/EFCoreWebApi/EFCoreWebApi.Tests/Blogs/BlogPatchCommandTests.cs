using EFCoreWebApi.Blogs;
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
        public void Invalid()
        {
            var blog = new BlogViewModel { };
            var results = new BlogPatchCommand(dbContext).Execute(null, blog.ToPatchOperations());
            results.AssertHasValidationResult("Name", "Name is required");

            blog.Name = "".PadRight(101, 'A');
            results = new BlogPatchCommand(dbContext).Execute(null, blog.ToPatchOperations());
            results.AssertHasValidationResult("Name", "Name length must be equal to or less than 100 characters");

            blog.Name = "Seed Blog";
            results = new BlogPatchCommand(dbContext).Execute(null, blog.ToPatchOperations());
            results.AssertHasValidationResult("Name", "Name of Seed Blog already exists");
        }

        [TestMethod]
        public void Update_NotFound()
        {
            Assert.ThrowsException<ItemNotFoundException>(() => new BlogPatchCommand(dbContext).Execute(111111, new BlogViewModel().ToPatchOperations()));
        }
    }
}
