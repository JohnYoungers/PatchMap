using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EFCoreAspNetCore.Web.Filters
{
    public class PatchCommandResultFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objResult && objResult.Value is PatchCommandResult saveResult)
            {
                if (saveResult.ValidationResults.Any())
                {
                    context.Result = new BadRequestObjectResult(saveResult.ValidationResults);
                }
                else
                {
                    var entity = saveResult.GetEntity();

                    if (saveResult.IsNew)
                    {
                        context.Result = new CreatedResult($"{context.HttpContext.Request.Path}/{saveResult.EntityId}", entity);
                    }
                    else if (entity != null)
                    {
                        context.Result = new OkObjectResult(entity);
                    }
                    else
                    {
                        context.Result = new NoContentResult();
                    }
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext context) { }
    }
}
