using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCoreAspNetCore.Blogs.Posts;
using EFCoreAspNetCore.Data;
using EFCoreAspNetCore.Framework;
using PatchMap;
using PatchMap.Mapping;

namespace EFCoreAspNetCore.Domain.Blogs
{
    public class BlogPatchCommand : PatchCommandBase<BlogViewModel, Blog, PatchContextBase>
    {
        static BlogPatchCommand()
        {
            mapper.AddMap(vm => vm.Name, db => db.Name).HasPostMap((target, ctx, map, operation) =>
            {
                //You could also do this check at the Converter stage opposed to the PostMap stage
                //However, the Converter will be run anytime there's a value, whereas PostMap only runs on change
                if (ctx.DbContext.Blogs.Any(b => b.Name == target.Name && b.BlogId != target.BlogId))
                {
                    ctx.AddValidationResult(operation, $"Name of {target.Name} already exists");
                }
            });
            mapper.AddMap(vm => vm.Url, db => db.Url);
            mapper.AddMap(vm => vm.PromotedPost, db => db.PromotedPostId).HasConverter((target, ctx, value) =>
            {
                var (match, failureReason) = value.ToEntityConverter().Convert(ctx.DbContext.Posts.Where(p => p.BlogId == target.BlogId && p.PostId == value.Id));

                target.PromotedPost = match;
                return new FieldMapConversionResult<int?> { Value = match?.PostId, FailureReason = failureReason };
            }).IsRequired((target, ctx) => !ctx.IsNew);

            //These don't need to be lambdas: you can split them out into their own functions if that's more clear
            mapper.AddMap(vm => vm.Tags).HasPostMap(PostMapTags);
        }

        private static void PostMapTags(Blog target, PatchContextBase ctx, FieldMap<Blog, PatchContextBase> map, PatchOperation operation)
        {
            var tags = operation.Value as List<string> ?? new List<string>();

            target.Tags.RemoveAll(existing => !tags.Any(t => string.Equals(existing.Name, t, StringComparison.CurrentCultureIgnoreCase)));
            target.Tags.AddRange(tags.Where(t => !target.Tags.Any(existing => string.Equals(existing.Name, t, StringComparison.CurrentCultureIgnoreCase)))
                                     .Select(t => new Tag { Name = t }));
        }

        public BlogPatchCommand(ExampleContext dbContext) : base(dbContext) { }

        public PatchCommandResult<BlogViewModel> Execute(int? id, List<PatchOperation> operations)
        {
            var (dbItem, isNew) = GetEntity(id,
                () => DbContext.Blogs.Where(b => b.BlogId == id),
                () => DbContext.Blogs.Add(new Blog()).Entity);

            var results = mapper.Map(operations, dbItem, GenerateContext(isNew));

            return GeneratePatchResult(dbItem, results, () =>
            {
                DbContext.SaveChanges();
                return new PatchCommandResult<BlogViewModel>(isNew, dbItem.BlogId.ToString(), Map(dbItem, BlogViewModel.Map));
            });
        }
    }
}
