using EF6AspNetWebApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Blogs
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
