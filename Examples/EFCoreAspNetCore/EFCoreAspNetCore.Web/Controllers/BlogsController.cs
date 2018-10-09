using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCoreAspNetCore.Blogs;
using EFCoreAspNetCore.Data;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using PatchMap;

namespace EFCoreAspNetCore.Web.Controllers
{
    [Route("api/[controller]")]
    public class BlogsController : BaseController
    {
        public BlogsController(ExampleContext dbContext) : base(dbContext) { }

        [HttpGet]
        public List<BlogViewModel> Search(ODataQueryOptions<Blog> query)
        {
            return new BlogSearchCommand(DbContext).Execute(query.ApplyTo);
        }

        [HttpGet("{id}")]
        public BlogViewModel Get(int id)
        {
            return new BlogGetCommand(DbContext).Execute(id);
        }

        [HttpPost]
        public void Insert(BlogViewModel blog)
        {
            new BlogPatchCommand(DbContext).Execute(null, blog.ToPatchOperations());
        }

        [HttpPut("{id}")]
        public void Update(int id, BlogViewModel blog)
        {
            new BlogPatchCommand(DbContext).Execute(id, blog.ToPatchOperations());
        }

        [HttpPatch("{id}")]
        public void Patch(int id, List<JsonPatch> patches)
        {
            new BlogPatchCommand(DbContext).Execute(id, patches.ToPatchOperations<BlogViewModel>());
        }
    }
}
