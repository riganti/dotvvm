using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;

namespace DotVVM.Tracing.ApplicationInsights.AspNetCore
{
    /// <summary>
    /// This class helps to inject Application Insights JavaScript snippet into dothtml.
    /// </summary>
    public class ApplicationInsightsJavascript : DotvvmControl
    {
        private readonly IOptions<TelemetryConfiguration> telemetryConfiguration;
        private readonly IOptions<ApplicationInsightsServiceOptions> applicationInsightsServiceOptions;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ApplicationInsightsJavascript(
            IOptions<TelemetryConfiguration> telemetryConfiguration, 
            IOptions<ApplicationInsightsServiceOptions> applicationInsightsServiceOptions,
            IHttpContextAccessor httpContextAccessor)
        {
            this.telemetryConfiguration = telemetryConfiguration;
            this.applicationInsightsServiceOptions = applicationInsightsServiceOptions;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var doNotTrack = false;
            if (context.HttpContext.Request.Headers.TryGetValue("DNT", out var doNotTrackHeaderValue))
            {
                doNotTrack = string.Equals(doNotTrackHeaderValue, "1");
            }

            if (!doNotTrack)
            {
                var javascriptSnippet = new JavaScriptSnippet(telemetryConfiguration.Value, applicationInsightsServiceOptions, httpContextAccessor);

                writer.WriteUnencodedText(javascriptSnippet.FullScript);
            }

            base.RenderControl(writer, context);
        }
    }
}
