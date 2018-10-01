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
                    context.HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                }
                else if (context.HttpContext.Request.Method == HttpMethods.Post)
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status201Created;
                }
            }
            else if (context.Result is EmptyResult)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            }
        }

        public void OnResultExecuted(ResultExecutedContext context) { }
    }
}
