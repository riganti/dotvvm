using System;
using System.Reflection;
using DotVVM.Core.Common;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class AddTypeToModelSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (model.Type == "object")
            {
                model.Extensions.Add(ApiConstants.DotvvmTypeKey, new OpenApiString(context.Type.FullName + ", " + context.Type.Assembly.GetName().Name));
            }
        }
    }
}
