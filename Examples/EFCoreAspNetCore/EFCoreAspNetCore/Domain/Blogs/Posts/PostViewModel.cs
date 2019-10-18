using DynamicExpressions.Mapping;
using EFCoreAspNetCore.Data;
using EFCoreAspNetCore.Domain.Blogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EFCoreAspNetCore.Domain.Blogs.Posts
{
    public class PostViewModel : PostSummaryViewModel
    {
        public BlogSummaryViewModel Blog { get; set; }
        public string Content { get; set; }

        public PostSummaryViewModel UpdatedPost { get; set; }
        public DateTimeOffset? UpdatedAsOfDate { get; set; }

        public new static Expression<Func<Post, PostViewModel>> Map = PostSummaryViewModel.Map.Concat(i => new PostViewModel
        {
            Blog = BlogSummaryViewModel.Map.Invoke(i.Blog),
            Content = i.Content,
            UpdatedPost = i.UpdatedPostId == null ? null : PostSummaryViewModel.Map.Invoke(i.UpdatedPost),
            UpdatedAsOfDate = i.UpdatedAsOfDate
        }).Flatten();
    }
}
