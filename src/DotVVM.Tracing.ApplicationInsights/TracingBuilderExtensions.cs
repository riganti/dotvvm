using DotVVM.Framework.Runtime.Tracing;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Tracing.ApplicationInsights
{
    public static class TracingBuilderExtensions
    {
        /// <summary>
        /// Registers TelemetryClient and ApplicationInsightsReporter
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IDotvvmOptions AddApplicationInsightsTracing(this IDotvvmOptions options)
        {
            options.Services.TryAddSingleton<TelemetryClient>();
            options.Services.TryAddSingleton<IRequestTracingReporter, ApplicationInsightsReporter>();
            return options;
        }
    }
}
