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
        /// <param name="options"></param>
        /// <returns></returns>
        internal static IDotvvmServiceCollection AddApplicationInsightsTracingInternal(this IDotvvmServiceCollection options)
        {
            var builder = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;
            builder.Use((next) => new RequestTelemetryFilter(next));
            builder.Build();

            options.TryAddSingleton<TelemetryClient>();         
            options.AddTransient<IRequestTracer, ApplicationInsightsTracer>();

            return options;
        }
    }
}
