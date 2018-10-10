using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace EF6AspNetWebApi.Web.Swashbuckle
{
    public class ODataOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (apiDescription.ParameterDescriptions.Where(p => p.ParameterDescriptor != null)
                                                    .Select(p => p.ParameterDescriptor.ParameterType)
                                                    .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(System.Web.OData.Query.ODataQueryOptions<>)))
            {
                operation.parameters.Add(new Parameter
                {
                    name = "$filter",
                    @in = "query",
                    description = "Filters the results based on an OData formatted condition.",
                    type = "string"
                });
                operation.parameters.Add(new Parameter
                {
                    name = "$orderby",
                    @in = "query",
                    description = "Sorts the results based on an OData formatted condition.",
                    type = "string"
                });
                operation.parameters.Add(new Parameter
                {
                    name = "$top",
                    @in = "query",
                    description = "Returns only the first n results.",
                    type = "integer"
                });
                operation.parameters.Add(new Parameter
                {
                    name = "$skip",
                    @in = "query",
                    description = "Skips the first n results.",
                    type = "integer"
                });
                operation.parameters.Add(new Parameter
                {
                    name = "$count",
                    @in = "query",
                    description = "Includes a count of the matching results in the X-ODATA-count header.",
                    type = "boolean"
                });
            }
        }
    }
}