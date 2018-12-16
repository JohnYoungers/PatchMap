using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCoreAspNetCore.Data;
using LinqKit;

namespace EFCoreAspNetCore.Blogs.Posts
{
    public class PostGetCommand : CommandBase
    {
        public PostGetCommand(ExampleContext dbContext) : base(dbContext) { }

        public PostViewModel Execute(int blogId, int id)
        {
            return FilterToFirstOrDefault(DbContext.Posts.Where(p => p.BlogId == blogId && p.PostId == id), PostViewModel.Map());
        }
    }
}
