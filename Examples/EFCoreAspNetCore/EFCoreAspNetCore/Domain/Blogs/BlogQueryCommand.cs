using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCoreAspNetCore.Data;
using EFCoreAspNetCore.Framework;

namespace EFCoreAspNetCore.Domain.Blogs
{
    public class BlogQueryCommand : CommandBase
    {
        public BlogQueryCommand(ExampleContext dbContext) : base(dbContext) { }

        public BlogViewModel Execute(int id)
        {
            return FilterToFirstOrDefault(DbContext.Blogs.Where(b => b.BlogId == id), BlogViewModel.Map);
        }

        public List<BlogViewModel> Execute(Func<IQueryable, IQueryable> filter = null)
        {
            return FilterToList(DbContext.Blogs, BlogViewModel.Map, filter);
        }
    }
}
