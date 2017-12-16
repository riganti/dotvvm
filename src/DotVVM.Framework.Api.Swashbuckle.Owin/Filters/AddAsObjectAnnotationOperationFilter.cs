using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using DotVVM.Framework.Api.Swashbuckle.Attributes;
using Swashbuckle.Swagger;

namespace DotVVM.Framework.Api.Swashbuckle.Owin.Filters
{
    public class AddAsObjectAnnotationOperationFilter : IOperationFilter 
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            foreach (var param in apiDescription.ParameterDescriptions.Where(d => d.ParameterDescriptor.GetCustomAttributes<AsObjectAttribute>().Any()))
            {
                // add full type name to the metadata
                foreach (var jsonParam in operation.parameters.Where(p => p.name.StartsWith(param.Name + ".")))
                {
                    // the vendorExtensions dictionary instance is reused, create a new one
                    var dict = jsonParam.vendorExtensions.ToDictionary(e => e.Key, e => e.Value);
                    dict.Add("x-dotvvm-wrapperType", param.ParameterDescriptor.ParameterType.FullName + ", " + param.ParameterDescriptor.ParameterType.Assembly.GetName().Name);
                    jsonParam.vendorExtensions = dict;
                }
            }
        }
    }
}
