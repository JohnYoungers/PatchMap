using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCoreWebApi.Data;
using LinqKit;

namespace EFCoreWebApi.Blogs
{
    public class BlogGetCommand : BaseCommand
    {
        public BlogGetCommand(ExampleContext dbContext) : base(dbContext) { }

        public BlogViewModel Execute(int id)
        {
            return FilterToFirstOrDefault(DbContext.Blogs.Where(b => b.BlogId == id), BlogViewModel.Map());
        }
    }
}
