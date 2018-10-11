using EF6AspNetWebApi.Exceptions;
using PatchMap.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace EF6AspNetWebApi.Web.Filters
{
    public class JsonPatchParseExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception is JsonPatchParseException ex)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    new[] { new ValidationResult(ex.Message, new[] { $"Patch {ex.Patch.path}" }) }
                );
            }
        }
    }
}