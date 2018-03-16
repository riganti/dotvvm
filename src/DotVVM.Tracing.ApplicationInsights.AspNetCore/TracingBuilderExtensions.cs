using DotVVM.Framework.Configuration;
using DotVVM.Tracing.ApplicationInsights;
using DotVVM.Tracing.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.Configuration
{
    public static class TracingBuilderExtensions
    {
        /// <summary>
        /// Registers ApplicationInsightsTracer
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IDotvvmServiceCollection AddApplicationInsightsTracing(this IDotvvmServiceCollection services)
        {
            services.AddApplicationInsightsTracingInternal();

            services.TryAddSingleton<JavaScriptSnippet>();
            services.AddTransient<IConfigureOptions<DotvvmConfiguration>, ApplicationInsightSetup>();

            return services;
        }
    }

    internal class ApplicationInsightSetup : IConfigureOptions<DotvvmConfiguration>
    {
        public void Configure(DotvvmConfiguration config)
        {
            config.Markup.AddCodeControls("dot", typeof(ApplicationInsightsJavascript));
        }
    }
}
