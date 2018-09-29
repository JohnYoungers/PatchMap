using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EFCoreWebApi.Blogs.Posts
{
    public class PostSummaryViewModel
    {
        public string Title { get; set; }
        public DateTimeOffset Created { get; set; }

        public static Expression<Func<Data.Post, PostSummaryViewModel>> Map()
        {
            return i => new PostSummaryViewModel
            {
                Title = i.Title,
                Created = i.DateCreated
            };
        }
    }

    public class PostViewModel : PostSummaryViewModel
    {
        public string Content { get; set; }

        public new static Expression<Func<Data.Post, PostViewModel>> Map()
        {
            return i => new PostViewModel
            {
                Title = i.Title,
                Created = i.DateCreated,
                Content = i.Content
            };
        }
    }
}
