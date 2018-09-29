using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCoreWebApi.Blogs;
using EFCoreWebApi.Data;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreWebApi.Web.Controllers
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
        public void Post([FromBody]string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
    }
}
