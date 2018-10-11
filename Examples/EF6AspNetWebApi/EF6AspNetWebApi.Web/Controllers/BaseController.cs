using EF6AspNetWebApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace EF6AspNetWebApi.Web.Controllers
{
    public class BaseController : ApiController
    {
        protected ExampleContext DbContext { get; private set; }

        public BaseController(ExampleContext dbContext)
        {
            DbContext = dbContext;
        }
    }
}