using EF6AspNetWebApi.Blogs;
using EF6AspNetWebApi.Data;
using Microsoft.AspNet.OData.Query;
using PatchMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace EF6AspNetWebApi.Web.Controllers
{
    [RoutePrefix("api/blogs")]
    public class BlogsController : BaseController
    {
        public BlogsController(ExampleContext dbContext) : base(dbContext) { }

        [HttpGet, Route("")]
        public List<BlogViewModel> Search(ODataQueryOptions<Blog> query)
        {
            return new BlogSearchCommand(DbContext).Execute(query.ApplyTo);
        }

        [HttpGet, Route("{id}")]
        public BlogViewModel Get(int id)
        {
            return new BlogGetCommand(DbContext).Execute(id);
        }

        [HttpPost, Route("")]
        public PatchCommandResult<BlogViewModel> Insert(BlogViewModel blog)
        {
            return new BlogPatchCommand(DbContext).Execute(null, blog.ToPatchOperations());
        }

        [HttpPut, Route("{id}")]
        public PatchCommandResult<BlogViewModel> Update(int id, BlogViewModel blog)
        {
            return new BlogPatchCommand(DbContext).Execute(id, blog.ToPatchOperations());
        }

        [HttpPatch, Route("{id}")]
        public PatchCommandResult<BlogViewModel> Patch(int id, List<JsonPatch> patches)
        {
            return new BlogPatchCommand(DbContext).Execute(id, patches.ToPatchOperations<BlogViewModel>());
        }
    }
}