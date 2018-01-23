using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using DotVVM.Framework.Controls;
using Swashbuckle.Swagger;

namespace DotVVM.Framework.Api.Swashbuckle.Owin.Filters
{
    public class HandleGridViewDataSetReturnType : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (apiDescription.ActionDescriptor.ReturnType?.IsGenericType == true
                && apiDescription.ActionDescriptor.ReturnType.GetGenericTypeDefinition() == typeof(GridViewDataSet<>))
            {
                var dict = operation.vendorExtensions.ToDictionary(e => e.Key, e => e.Value);
                dict.Add("x-dotvvm-returnsDataSet", "true");
                operation.vendorExtensions = dict;
            }
        }
    }
}
