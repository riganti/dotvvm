using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotVVM.Tracing.ApplicationInsights.Owin
{
    public static class TracingBuilderExtensions
    {
        /// <summary>
        /// Registers ApplicationInsightsTracer
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IDotvvmServiceCollection AddApplicationInsightsTracing(this IDotvvmServiceCollection options)
        {
            options.AddApplicationInsightsTracingInternal();
            options.AddTransient<IConfigureOptions<DotvvmConfiguration>, ApplicationInsightSetup>();

            return options;
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
