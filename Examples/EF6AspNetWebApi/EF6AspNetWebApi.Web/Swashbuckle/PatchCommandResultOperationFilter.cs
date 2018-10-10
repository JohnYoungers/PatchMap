using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Description;

namespace EF6AspNetWebApi.Web.Swashbuckle
{
    public class PatchCommandResultOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var returnType = apiDescription.ActionDescriptor.ReturnType;
            if (returnType != null && returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(PatchCommandResult<>))
            {
                operation.responses.Clear();
                operation.responses.Add("400", new Response
                {
                    description = "Validation Failed",
                    schema = schemaRegistry.GetOrRegister(typeof(ValidationResult[]))
                });

                var isInsert = apiDescription.HttpMethod == HttpMethod.Post;
                operation.responses.Add(isInsert ? "201" : "200", new Response
                {
                    description = isInsert ? "Inserted" : "Updated",
                    schema = schemaRegistry.GetOrRegister(returnType.GetGenericArguments()[0])
                });
            }
        }
    }
}