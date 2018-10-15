using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFCoreAspNetCore.Web.Swashbuckle
{
    public class ODataOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ParameterDescriptions.Where(p => p.ParameterDescriptor != null)
                                                    .Select(p => p.ParameterDescriptor.ParameterType)
                                                    .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Microsoft.AspNet.OData.Query.ODataQueryOptions<>)))
            {
                //Need to figure out how to prevent ODataQueryOptions from being included in the schema in general
                operation.Parameters.Clear();

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$filter",
                    In = "query",
                    Description = "Filters the results based on an OData formatted condition.",
                    Type = "string"
                });
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$orderby",
                    In = "query",
                    Description = "Sorts the results based on an OData formatted condition.",
                    Type = "string"
                });
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$top",
                    In = "query",
                    Description = "Returns only the first n results.",
                    Type = "integer"
                });
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$skip",
                    In = "query",
                    Description = "Skips the first n results.",
                    Type = "integer"
                });
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$count",
                    In = "query",
                    Description = "Includes a count of the matching results in the X-ODATA-count header.",
                    Type = "boolean"
                });
            }
        }
    }
}
