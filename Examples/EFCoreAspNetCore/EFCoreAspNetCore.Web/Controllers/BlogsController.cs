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
            return new BlogQueryCommand(DbContext).Execute(query.ApplyTo);
        }

        [HttpGet("{id}")]
        public BlogViewModel Get(int id)
        {
            return new BlogQueryCommand(DbContext).Execute(id);
        }

        [HttpPost]
        public PatchCommandResult<BlogViewModel> Insert([FromBody]BlogViewModel blog)
        {
            return new BlogPatchCommand(DbContext).Execute(null, blog.ToPatchOperations());
        }

        [HttpPut("{id}")]
        public PatchCommandResult<BlogViewModel> Update(int id, [FromBody]BlogViewModel blog)
        {
            return new BlogPatchCommand(DbContext).Execute(id, blog.ToPatchOperations());
        }

        [HttpPatch("{id}")]
        public PatchCommandResult<BlogViewModel> Patch(int id, [FromBody]List<JsonPatch> patches)
        {
            return new BlogPatchCommand(DbContext).Execute(id, patches.ToPatchOperations<BlogViewModel>());
        }
    }
}
