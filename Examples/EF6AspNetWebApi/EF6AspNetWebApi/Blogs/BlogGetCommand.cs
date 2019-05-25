using EF6AspNetWebApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Blogs
{
    public class BlogGetCommand : CommandBase
    {
        public BlogGetCommand(ExampleContext dbContext) : base(dbContext) { }

        public BlogViewModel Execute(int id)
        {
            return FilterToFirstOrDefault(DbContext.Blogs.Where(b => b.BlogId == id), BlogViewModel.Map());
        }
    }
}
