using System;
using DotVVM.Framework.Api.Swashbuckle.Owin.Filters;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace DotVVM.Framework.Api.Swashbuckle.Owin
{
    public static class SwashbuckleExtensions
    {
        /// <summary>
        /// Confgures Swaschbuckle to provide additional metadata in methods which use FromQuery attribute so the API provided by DotVVM API generator is easier to use.
        /// </summary>
        public static void EnableDotvvmIntegration(this SwaggerDocsConfig options)
        {
            options.OperationFilter<AddAsObjectAnnotationOperationFilter>();
            options.OperationFilter<HandleGridViewDataSetReturnType>();
        }

    }
}
