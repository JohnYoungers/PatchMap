using EF6AspNetWebApi.Data;
using LinqKit;
using PatchMap;
using PatchMap.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Blogs.Posts
{
    public class PostPatchCommand : BasePatchCommand<PostViewModel, Post, BasePatchContext>
    {
        static PostPatchCommand()
        {
            //Post details can only be set on create
            mapper.AddMap(vm => vm.Title, db => db.Title).IsEnabled((t, ctx) => ctx.IsNew);
            mapper.AddMap(vm => vm.Content, db => db.Content).IsEnabled((t, ctx) => ctx.IsNew);

            //Group related fields that need to be validated together with a CompoundMap
            mapper.AddCompoundMap(cm =>
            {
                cm.AddMap(vm => vm.UpdatedAsOfDate, db => db.UpdatedAsOfDate);
                cm.AddMap(vm => vm.UpdatedPost, db => db.UpdatedPostId).HasConverter((target, ctx, value) =>
                {
                    var (match, failureReason) = value.ToEntityConverter().Convert(target.Blog.Posts.AsQueryable().Where(p => p.PostId == value.Id));

                    target.UpdatedPost = match;
                    return new FieldMapConversionResult<int?> { Value = match?.PostId, FailureReason = failureReason };
                });
            }, (target, ctx) =>
            {
                if ((target.UpdatedAsOfDate.HasValue && !target.UpdatedPostId.HasValue) || (!target.UpdatedAsOfDate.HasValue && target.UpdatedPostId.HasValue))
                {
                    ctx.AddValidationResult(nameof(PostViewModel.UpdatedPost), $"Updated Post requires both a Post and an As Of date");
                }
            });
        }

        public PostPatchCommand(ExampleContext dbContext) : base(dbContext) { }

        public PatchCommandResult<PostViewModel> Execute(int blogId, int? id, List<PatchOperation> operations)
        {
            var dbBlog = EnsureExists(DbContext.Blogs.FirstOrDefault(b => b.BlogId == blogId));

            var (dbItem, isNew) = GetEntity(id,
                () => dbBlog.Posts.Where(b => b.PostId == id),
                () =>
                {
                    var dbPost = new Post { Blog = dbBlog, DateCreated = DateTimeOffset.Now };
                    dbBlog.Posts.Add(dbPost);

                    return dbPost;
                });

            var results = mapper.Map(operations, dbItem, GenerateContext(isNew));

            return GeneratePatchResult(results, () =>
            {
                DbContext.SaveChanges();
                return (isNew, dbItem.PostId.ToString(), PostViewModel.Map().Invoke(dbItem));
            });
        }
    }
}
