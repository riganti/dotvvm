using System.Diagnostics;
using DotVVM.Framework.Hosting;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Tracing.ApplicationInsights.Owin
{
    public static class TracingBuilderExtensions
    {
        public static IDotvvmOptions AddApplicationInsightsTracing(this IDotvvmOptions options)
        {
            options.Services.TryAddScoped<TelemetryClient>();
            options.Configuration?.Runtime.Reporters.Add(new ApplicationInsightsReporter());
            return options;
        }
    }
}
