using System;
using System.Linq.Expressions;

namespace EFCoreAspNetCore.Domain.Blogs
{
    public class BlogSummaryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public static Expression<Func<Data.Blog, BlogSummaryViewModel>> Map = i => new BlogSummaryViewModel
        {
            Id = i.BlogId,
            Name = i.Name,
            Url = i.Url
        };
    }
}
