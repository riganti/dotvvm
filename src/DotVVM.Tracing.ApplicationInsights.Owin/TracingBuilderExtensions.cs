using DotVVM.Tracing.ApplicationInsights;
using DotVVM.Tracing.ApplicationInsights.Owin;
using Microsoft.ApplicationInsights;
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
            TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Use(next => new RequestTelemetryFilter(next)).Build();

            services.Services.TryAddSingleton<TelemetryClient>();
            services.AddDotvvmApplicationInsights();
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
