using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCoreWebApi.Data;
using LinqKit;

namespace EFCoreWebApi.Blogs.Posts
{
    public class PostSearchCommand : BaseCommand
    {
        public PostSearchCommand(ExampleContext dbContext) : base(dbContext) { }

        public List<PostViewModel> Execute(int blogId, Func<IQueryable, IQueryable> filter = null)
        {
            return FilterToList(DbContext.Blogs.Where(b => b.BlogId == blogId).SelectMany(b => b.Posts), filter, PostViewModel.Map());
        }
    }
}
