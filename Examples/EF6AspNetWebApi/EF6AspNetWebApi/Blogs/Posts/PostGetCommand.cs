using EF6AspNetWebApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Blogs.Posts
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
