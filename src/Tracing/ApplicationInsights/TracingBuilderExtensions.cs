using System;
using System.Linq;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Tracing.ApplicationInsights
{
    public static class TracingBuilderExtensions
    {
        /// <summary>
        /// Registers ApplicationInsightsTracer
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        internal static IDotvvmServiceCollection AddDotvvmApplicationInsights(this IDotvvmServiceCollection services)
        {
            services.Services.AddTransient<IRequestTracer, ApplicationInsightsTracer>();
            
            return services;
        }
    }
}
