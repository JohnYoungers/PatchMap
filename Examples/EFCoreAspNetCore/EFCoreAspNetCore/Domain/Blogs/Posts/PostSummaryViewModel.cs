using EFCoreAspNetCore.Data;
using EFCoreAspNetCore.Framework;
using System;
using System.Linq.Expressions;

namespace EFCoreAspNetCore.Domain.Blogs.Posts
{
    public class PostSummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTimeOffset Created { get; set; }

        public static Expression<Func<Post, PostSummaryViewModel>> Map = i => new PostSummaryViewModel
        {
            Id = i.PostId,
            Title = i.Title,
            Created = i.DateCreated
        };

        public ViewModelToEntity<Post> ToEntityConverter()
        {
            return new ViewModelToEntity<Post>(
                (Id != default, i => i.PostId == Id),
                (!string.IsNullOrEmpty(Title), i => i.Title == Title),
                (Created != default, i => i.DateCreated == Created)
            );
        }
    }
}
