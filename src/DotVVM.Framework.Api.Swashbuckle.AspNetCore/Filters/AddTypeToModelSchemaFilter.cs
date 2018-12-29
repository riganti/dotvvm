using System;
using System.Reflection;
using DotVVM.Core.Common;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class AddTypeToModelSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            if (model.Type == "object")
            {
                model.Extensions.Add(ApiConstants.DotvvmTypeKey, context.SystemType);
            }
        }
    }
}
