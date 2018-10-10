using EFCoreAspNetCore.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EFCoreAspNetCore.Web.Filters
{
    public class ItemNotFoundExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ItemNotFoundException)
            {
                context.Result = new NotFoundObjectResult(context.Exception.Message);
            }
        }
    }
}
