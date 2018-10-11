using EFCoreAspNetCore.Data;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EFCoreAspNetCore.Blogs.Posts
{
    public class PostSummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTimeOffset Created { get; set; }

        public static Expression<Func<Post, PostSummaryViewModel>> Map()
        {
            return i => new PostSummaryViewModel
            {
                Id = i.PostId,
                Title = i.Title,
                Created = i.DateCreated
            };
        }

        public ViewModelToEntity<Post> ToEntityConverter()
        {
            return new ViewModelToEntity<Post>(
                (Id != default, i => i.PostId == Id),
                (!string.IsNullOrEmpty(Title), i => i.Title == Title),
                (Created != default, i => i.DateCreated == Created)
            );
        }
    }

    public class PostViewModel : PostSummaryViewModel
    {
        public BlogSummaryViewModel Blog { get; set; }
        public string Content { get; set; }

        public PostSummaryViewModel UpdatedPost { get; set; }
        public DateTimeOffset? UpdatedAsOfDate { get; set; }

        public new static Expression<Func<Post, PostViewModel>> Map()
        {
            var blogMap = BlogSummaryViewModel.Map();
            var postMap = PostSummaryViewModel.Map();

            return i => new PostViewModel
            {
                Id = i.PostId,
                Title = i.Title,
                Created = i.DateCreated,
                Blog = blogMap.Invoke(i.Blog),
                Content = i.Content,
                UpdatedPost = (i.UpdatedPostId == null) ? null : postMap.Invoke(i.UpdatedPost),
                UpdatedAsOfDate = i.UpdatedAsOfDate
            };
        }
    }
}
