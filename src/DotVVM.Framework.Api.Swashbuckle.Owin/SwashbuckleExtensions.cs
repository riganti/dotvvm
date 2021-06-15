using System;
using DotVVM.Core.Common;
using DotVVM.Framework.Api.Swashbuckle.Owin.Filters;
using DotVVM.Framework.ViewModel;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace DotVVM.Framework.Api.Swashbuckle.Owin
{
    public static class SwashbuckleExtensions
    {
        /// <summary>
        /// Configures Swashbuckle to provide additional metadata in methods which use FromQuery attribute so the API provided by DotVVM API generator is easier to use.
        /// </summary>
        public static void EnableDotvvmIntegration(this SwaggerDocsConfig options, Action<DotvvmApiOptions> configureOptions = null)
        {
            var apiOptions = new DotvvmApiOptions();
            configureOptions?.Invoke(apiOptions);

            var propertySerialization = new DefaultPropertySerialization();
            options.OperationFilter(() => new AddAsObjectAnnotationOperationFilter(propertySerialization));
            options.SchemaFilter(() => new AddTypeToModelSchemaFilter());
            options.DocumentFilter(() => new HandleKnownTypesDocumentFilter(apiOptions, propertySerialization));
        }
    }
}
