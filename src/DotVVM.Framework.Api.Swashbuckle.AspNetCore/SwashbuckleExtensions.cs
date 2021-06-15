using System;
using System.Linq;
using DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters;
using DotVVM.Framework.Controls;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore
{
    public static class SwashbuckleExtensions
    {
        /// <summary>
        /// Configures Swashbuckle to provide additional metadata in methods which use FromQuery attribute so the API provided by DotVVM API generator is easier to use.
        /// </summary>
        public static void EnableDotvvmIntegration(this SwaggerGenOptions options)
        {
            options.OperationFilterDescriptors.Add(new FilterDescriptor() { Type = typeof(RemoveReadOnlyFromUriParametersOperationFilter), Arguments = new object[] { } });
            options.OperationFilterDescriptors.Add(new FilterDescriptor() { Type = typeof(RemoveBindNoneFromUriParametersOperationFilter), Arguments = new object[] { } });
            options.OperationFilterDescriptors.Add(new FilterDescriptor() { Type = typeof(AddAsObjectOperationFilter), Arguments = new object[] { } });

            options.SchemaFilterDescriptors.Add(new FilterDescriptor() { Type = typeof(AddTypeToModelSchemaFilter), Arguments = new object[] { } });
            options.DocumentFilterDescriptors.Add(new FilterDescriptor() { Type = typeof(HandleKnownTypesDocumentFilter), Arguments = new object[] { } });

            options.CustomSchemaIds(type => GetCustomSchemaId(type));
        }

        private static string GetCustomSchemaId(Type modelType)
        {
            if (!modelType.IsConstructedGenericType) return modelType.Name.Replace("[]", "Array");

            var generics = modelType.GetGenericArguments()
                .Select(genericArg => GetCustomSchemaId(genericArg))
                .Aggregate((previous, current) => previous + current);

            return $"{modelType.Name.Split('`').First()}[{generics}]";
        }
    }
}
