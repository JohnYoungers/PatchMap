using EFCoreAspNetCore.Framework;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EFCoreAspNetCore.Web.Swashbuckle
{
    public class PatchCommandResultOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                var returnType = descriptor.MethodInfo.ReturnType;
                if (returnType != null && returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(PatchCommandResult<>))
                {
                    operation.Responses.Clear();
                    operation.Responses.Add("400", new Response
                    {
                        Description = "Validation Failed",
                        Schema = context.SchemaRegistry.GetOrRegister(typeof(ValidationResult[]))
                    });

                    var isInsert = context.ApiDescription.HttpMethod == "POST";
                    operation.Responses.Add(isInsert ? "201" : "200", new Response
                    {
                        Description = isInsert ? "Inserted" : "Updated",
                        Schema = context.SchemaRegistry.GetOrRegister(returnType.GetGenericArguments()[0])
                    });
                }
            }
        }
    }
}
