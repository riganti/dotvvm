using System;
using DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters;
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
            options.OperationFilter<AddFromUriParameterGroupsOperationFilter>();
            options.OperationFilter<RemoveReadOnlyFromUriParametersOperationFilter>();
            options.OperationFilter<RemoveBindNoneFromUriParametersOperationFilter>();
        }

    }
}
