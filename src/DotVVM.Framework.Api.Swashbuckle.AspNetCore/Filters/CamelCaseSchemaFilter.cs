using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class CamelCaseSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            schema.Properties = schema.Properties.ToDictionary(
                i => i.Key.Substring(0, 1).ToLower() + i.Key.Substring(1), i => i.Value);
        }
    }
}
