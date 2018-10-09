using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCoreWebApi.Data;
using LinqKit;

namespace EFCoreWebApi.Blogs.Posts
{
    public class PostGetCommand : BaseCommand
    {
        public PostGetCommand(ExampleContext dbContext) : base(dbContext) { }

        public PostViewModel Execute(int blogId, int id)
        {
            return FilterToFirstOrDefault(DbContext.Posts.Where(p => p.BlogId == blogId && p.PostId == id), PostViewModel.Map());
        }
    }
}
