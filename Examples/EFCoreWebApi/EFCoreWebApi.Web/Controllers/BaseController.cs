using EFCoreWebApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreWebApi.Web.Controllers
{
    public class BaseController : Controller
    {
        protected ExampleContext DbContext { get; private set; }

        public BaseController(ExampleContext dbContext)
        {
            DbContext = dbContext;
        }
    }
}
