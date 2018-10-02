using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Reflection;

namespace EFCoreWebApi.Web.Filters
{
    public class SuccessCodeResultFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objResult)
            {
                if (objResult.Value == null)
                {
                    context.Result = new NoContentResult();
                }
                else if (objResult.Value is PatchCommandResult patchCommandResult)
                {
                    var entity = patchCommandResult.GetEntity();
                    if (!patchCommandResult.Succeeded)
                    {
                        context.Result = new BadRequestObjectResult(patchCommandResult.ValidationResults);
                    }
                    if (patchCommandResult.IsNew)
                    {
                        context.Result = new CreatedResult($"{context.HttpContext.Request.Path}/{patchCommandResult.EntityLocationId}", entity);
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
            else if (context.Result is EmptyResult)
            {
                context.Result = new NoContentResult();
            }
        }

        public void OnResultExecuted(ResultExecutedContext context) { }
    }
}
