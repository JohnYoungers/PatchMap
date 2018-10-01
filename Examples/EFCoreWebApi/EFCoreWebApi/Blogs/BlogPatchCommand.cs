using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCoreWebApi.Data;
using LinqKit;
using PatchMap;

namespace EFCoreWebApi.Blogs
{
    public class BlogPatchCommand : BasePatchCommand<BlogViewModel, Blog, BaseContext>
    {
        static BlogPatchCommand()
        {
            InitializeMapper();
            mapper.AddMap(vm => vm.Name, db => db.Name).HasPostMap(NamePostMap);
        }

        private static void NamePostMap(Blog target, BaseContext ctx, PatchOperation operation)
        {
            if (ctx.DbContext.Blogs.Any(b => b.Name == target.Name && b.BlogId != target.BlogId))
            {
                ctx.AddValidationResult(operation, $"Name of {target.Name} already exists");
            }
        }

        public BlogPatchCommand(ExampleContext dbContext) : base(dbContext) { }

        public PatchCommandResult<BlogViewModel> Execute(int? id, List<PatchOperation> operations)
        {
            var (dbItem, isNew) = GetEntity(id,
                () => DbContext.Blogs.Where(b => b.BlogId == id),
                () => DbContext.Blogs.Add(new Blog()).Entity);

            var results = mapper.Map(operations, dbItem, GenerateContext(isNew));

            return GeneratePatchResult(results, () =>
            {
                DbContext.SaveChanges();
                return new BlogViewModel();
            });
        }
    }
}
