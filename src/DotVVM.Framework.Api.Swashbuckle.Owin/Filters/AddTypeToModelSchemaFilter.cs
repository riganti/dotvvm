using System;
using System.Reflection;
using DotVVM.Core.Common;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Swashbuckle.Swagger;

namespace DotVVM.Framework.Api.Swashbuckle.Owin.Filters
{
    public class AddTypeToModelSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (schema.type == "object")
            {
                schema.vendorExtensions.Add(ApiConstants.DotvvmTypeKey, type);
            }
        }
    }
}
