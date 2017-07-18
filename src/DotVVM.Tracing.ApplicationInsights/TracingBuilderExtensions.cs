using System;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
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
        /// Registers TelemetryClient, ApplicationInsightsTracer and ApplicationInsightExceptionFilter
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IDotvvmOptions AddApplicationInsightsTracing(this IDotvvmOptions options)
        {
            var builder = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;
            builder.Use((next) => new RequestTelemetryFilter(next));

            builder.Build();

            options.Services.AddTransient<IRequestTracer, ApplicationInsightsTracer>();
            //options.Services.AddSingleton<Func<IRequestTracer>, Func<ApplicationInsightsTracer>>();

            //options.Services.TryAddSingleton<Func<IRequestTracer>, Func<ApplicationInsightsTracer>>();
            options.Services.AddTransient<Func<IRequestTracer>>((c) => () => c.GetRequiredService<ApplicationInsightsTracer>());
            //options.Configuration.Runtime.GlobalFilters.Add(new ApplicationInsightExceptionFilter());

            return options;
        }

        public static DotvvmConfiguration AddApplicationInsightsTracing(this DotvvmConfiguration config)
        {
            var telemetryClient = config.ServiceLocator.GetService<TelemetryClient>();
            var stopwatch = config.ServiceLocator.GetService<IStopwatch>();

            config.Runtime.TracerFactories.Add(() => new ApplicationInsightsTracer(telemetryClient, stopwatch));

            config.Runtime.GlobalFilters.Add(new ApplicationInsightExceptionFilter(telemetryClient));

            return config;
        }
    }
}
