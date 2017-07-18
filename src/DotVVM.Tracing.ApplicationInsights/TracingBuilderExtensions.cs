using System;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DotVVM.Tracing.ApplicationInsights
{
    public static class TracingBuilderExtensions
    {
        /// <summary>
        /// Registers ApplicationInsightsTracer
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IDotvvmOptions AddApplicationInsightsTracing(this IDotvvmOptions options)
        {
            var builder = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;
            builder.Use((next) => new RequestTelemetryFilter(next));
            builder.Build();

            options.Services.TryAddSingleton<TelemetryClient>();

            options.Services.AddTransient<ApplicationInsightsTracer>();
            //options.Services.AddSingleton<Func<IRequestTracer>>((c) => () => c.GetRequiredService<ApplicationInsightsTracer>());
            options.Services.AddTransient<IConfigureOptions<DotvvmConfiguration>, ApplicationInsightSetup>();

            return options;
        }
    }

    internal class ApplicationInsightSetup : IConfigureOptions<DotvvmConfiguration>
    {
        public void Configure(DotvvmConfiguration config)
        {
            var serviceProvider = config.ServiceLocator.GetServiceProvider();
            config.Runtime.TracerFactories.Add(() => serviceProvider.GetRequiredService<ApplicationInsightsTracer>());
        }
    }
}
