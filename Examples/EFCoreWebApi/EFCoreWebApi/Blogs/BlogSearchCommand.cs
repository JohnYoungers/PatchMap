using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCoreWebApi.Data;
using LinqKit;

namespace EFCoreWebApi.Blogs
{
    public class BlogSearchCommand : BaseCommand
    {
        public BlogSearchCommand(ExampleContext dbContext) : base(dbContext) { }

        public List<BlogViewModel> Execute(Func<IQueryable, IQueryable> filter = null)
        {
            return FilterToList(DbContext.Blogs, filter, BlogViewModel.Map());
        }
    }
}
