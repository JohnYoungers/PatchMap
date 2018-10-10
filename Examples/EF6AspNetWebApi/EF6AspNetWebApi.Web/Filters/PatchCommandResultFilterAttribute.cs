using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace EF6AspNetWebApi.Web.Filters
{
    public class PatchCommandResultFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response?.Content is ObjectContent objResult && objResult.Value is PatchCommandResult saveResult)
            {
                var request = actionExecutedContext.Request;
                if (!saveResult.Succeeded)
                {
                    actionExecutedContext.Response = request.CreateResponse(HttpStatusCode.BadRequest, saveResult.ValidationResults);
                }
                else
                {
                    var entity = saveResult.GetEntity();

                    if (saveResult.IsNew)
                    {
                        actionExecutedContext.Response = request.CreateResponse(HttpStatusCode.Created, entity);
                        actionExecutedContext.Response.Headers.Add("Location", $"{actionExecutedContext.Request.RequestUri.AbsolutePath}/{saveResult.EntityId}");
                    }
                    else if (entity != null)
                    {
                        actionExecutedContext.Response = request.CreateResponse(HttpStatusCode.OK, entity);
                    }
                    else
                    {
                        actionExecutedContext.Response = request.CreateResponse(HttpStatusCode.NoContent);
                    }
                }
            }
        }
    }
}