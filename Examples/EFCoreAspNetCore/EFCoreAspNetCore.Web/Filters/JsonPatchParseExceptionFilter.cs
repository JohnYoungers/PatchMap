using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PatchMap.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace EFCoreAspNetCore.Web.Filters
{
    public class JsonPatchParseExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is JsonPatchParseException ex)
            {
                context.Result = new BadRequestObjectResult(new[] { new ValidationResult(ex.Message, new[] { $"Patch {ex.Patch.path}" }) });
            }
        }
    }
}
