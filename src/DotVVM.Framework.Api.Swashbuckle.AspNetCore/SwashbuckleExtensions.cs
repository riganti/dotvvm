using System;
using DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters;
using DotVVM.Framework.Controls;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore
{
    public static class SwashbuckleExtensions
    {
        /// <summary>
        /// Confgures Swaschbuckle to provide additional metadata in methods which use FromQuery attribute so the API provided by DotVVM API generator is easier to use.
        /// </summary>
        public static void EnableDotvvmIntegration(this SwaggerGenOptions options)
        {
            options.OperationFilterDescriptors.Add(new FilterDescriptor() { Type = typeof(RemoveReadOnlyFromUriParametersOperationFilter), Arguments = new object[] { } });
            options.OperationFilterDescriptors.Add(new FilterDescriptor() { Type = typeof(RemoveBindNoneFromUriParametersOperationFilter), Arguments = new object[] { } });
            options.OperationFilterDescriptors.Add(new FilterDescriptor() { Type = typeof(AddAsObjectOperationFilter), Arguments = new object[] { } });

            options.SchemaFilterDescriptors.Add(new FilterDescriptor() { Type = typeof(AddTypeToModelSchemaFilter), Arguments = new object[] { } });
            options.DocumentFilterDescriptors.Add(new FilterDescriptor() { Type = typeof(HandleKnownTypesDocumentFilter), Arguments = new object[] { } });
        }
    }
}
