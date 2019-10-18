using DynamicExpressions.Mapping;
using EFCoreAspNetCore.Domain.Blogs.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EFCoreAspNetCore.Domain.Blogs
{
    public class BlogViewModel : BlogSummaryViewModel
    {
        public List<string> Tags { get; set; } = new List<string>();
        public List<PostSummaryViewModel> Posts { get; set; } = new List<PostSummaryViewModel>();
        public PostSummaryViewModel PromotedPost { get; set; }

        public new static Expression<Func<Data.Blog, BlogViewModel>> Map = BlogSummaryViewModel.Map.Concat(i => new BlogViewModel
        {
            Tags = i.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList(),
            Posts = i.Posts.OrderByDescending(p => p.DateCreated).Select(p => PostSummaryViewModel.Map.Invoke(p)).ToList(),
            PromotedPost = i.PromotedPostId == null ? null : PostSummaryViewModel.Map.Invoke(i.PromotedPost)
        }).Flatten();
    }
}
