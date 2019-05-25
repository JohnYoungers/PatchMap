using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCoreAspNetCore.Data;
using LinqKit;

namespace EFCoreAspNetCore.Blogs
{
    public class BlogGetCommand : CommandBase
    {
        public BlogGetCommand(ExampleContext dbContext) : base(dbContext) { }

        public BlogViewModel Execute(int id)
        {
            return FilterToFirstOrDefault(DbContext.Blogs.Where(b => b.BlogId == id), BlogViewModel.Map());
        }

        public List<BlogViewModel> Execute(Func<IQueryable, IQueryable> filter = null)
        {
            return FilterToList(DbContext.Blogs, filter, BlogViewModel.Map());
        }
    }
}
