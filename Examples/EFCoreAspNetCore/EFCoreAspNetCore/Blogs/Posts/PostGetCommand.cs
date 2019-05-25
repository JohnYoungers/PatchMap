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

        public List<PostViewModel> Execute(int blogId, Func<IQueryable, IQueryable> filter = null)
        {
            return FilterToList(DbContext.Blogs.Where(b => b.BlogId == blogId).SelectMany(b => b.Posts), filter, PostViewModel.Map());
        }
    }
}
