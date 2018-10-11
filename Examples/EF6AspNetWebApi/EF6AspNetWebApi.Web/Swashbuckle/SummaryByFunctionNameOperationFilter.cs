using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace EF6AspNetWebApi.Web.Swashbuckle
{
    public class SummaryByFunctionNameOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            operation.summary = apiDescription.ActionDescriptor.ActionName;
        }
    }
}