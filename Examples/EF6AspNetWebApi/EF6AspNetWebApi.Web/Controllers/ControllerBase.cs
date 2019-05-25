using EF6AspNetWebApi.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace EF6AspNetWebApi.Web.Controllers
{
    public class ControllerBase : ApiController
    {
        protected ILogger Logger { get; private set; }
        protected ExampleContext DbContext { get; private set; }

        public ControllerBase() { }
        public ControllerBase(ExampleContext dbContext) => DbContext = dbContext;
        public ControllerBase(ExampleContext dbContext, ILogger logger) : this(dbContext) => Logger = logger;
    }
}