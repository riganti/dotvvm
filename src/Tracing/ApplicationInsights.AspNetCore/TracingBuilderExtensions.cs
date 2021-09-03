using System;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Tracing.ApplicationInsights;
using DotVVM.Tracing.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
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
            if (!services.Services.Any(s => s.ServiceType == typeof(TelemetryClient)))
            {
                throw new InvalidOperationException("Application Insights must be configured before DotVVM. Make sure you call services.AddApplicationInsightsTelemetry(configuration) before services.AddDotVVM<DotvvmStartup>().");
            }

            services.Services.AddHttpContextAccessor();
            services.AddDotvvmApplicationInsights();

            services.Services.AddApplicationInsightsTelemetryProcessor<RequestTelemetryFilter>();

            services.Services.TryAddSingleton<JavaScriptSnippet>();
            services.Services.AddTransient<IConfigureOptions<DotvvmConfiguration>, ApplicationInsightSetup>();

            return services;
        }
    }

    internal class ApplicationInsightSetup : IConfigureOptions<DotvvmConfiguration>
    {
        public void Configure(DotvvmConfiguration options)
        {
            options.Markup.AddCodeControls("dot", typeof(ApplicationInsightsJavascript));
        }
    }
}
