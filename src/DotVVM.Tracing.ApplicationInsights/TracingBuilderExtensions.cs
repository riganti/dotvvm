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
        internal static IDotvvmServiceCollection AddApplicationInsightsTracingInternal(this IDotvvmServiceCollection services)
        {
            var builder = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;
            builder.Use((next) => new RequestTelemetryFilter(next));
            builder.Build();

            services.TryAddSingleton<TelemetryClient>();         
            services.AddTransient<IRequestTracer, ApplicationInsightsTracer>();

            return services;
        }
    }
}
