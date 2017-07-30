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
        internal static IDotvvmOptions AddApplicationInsightsTracingInternal(this IDotvvmOptions options)
        {
            var builder = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;
            builder.Use((next) => new RequestTelemetryFilter(next));
            builder.Build();

            options.Services.TryAddSingleton<TelemetryClient>();         
            options.Services.TryAddTransient<IRequestTracer, ApplicationInsightsTracer>();

            return options;
        }
    }
}
