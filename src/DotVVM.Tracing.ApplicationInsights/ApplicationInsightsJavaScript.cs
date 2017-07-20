using System.Globalization;
using System.Security.Principal;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DotVVM.Tracing.ApplicationInsights
{
    /// <summary>
    /// This class helps to inject Application Insights JavaScript snippet into dothtml.
    /// </summary>
    public class ApplicationInsightJavascript : DotvvmControl
    {
        /// <summary>JavaScript snippet.</summary>
        private static readonly string Snippet = Microsoft.ApplicationInsights.AspNetCore.Resources.JavaScriptSnippet;

        /// <summary>JavaScript authenticated user tracking snippet.</summary>
        private static readonly string AuthSnippet = Microsoft.ApplicationInsights.AspNetCore.Resources.JavaScriptAuthSnippet;

        /// <summary>Configuration instance.</summary>
        private TelemetryConfiguration telemetryConfiguration;

        /// <summary> Http context accessor.</summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary> Weather to print authenticated user tracking snippet.</summary>
        private bool enableAuthSnippet;

        /// <summary>
        /// Initializes a new instance of the ApplicationInsightJavascript class.
        /// </summary>
        /// <param name="telemetryConfiguration">The configuration instance to use.</param>
        /// <param name="serviceOptions">Service options instance to use.</param>
        /// <param name="httpContextAccessor">Http context accessor to use.</param>
        public ApplicationInsightJavascript(TelemetryConfiguration telemetryConfiguration,
            IOptions<ApplicationInsightsServiceOptions> serviceOptions,
            IHttpContextAccessor httpContextAccessor)
        {
            this.telemetryConfiguration = telemetryConfiguration;
            this.httpContextAccessor = httpContextAccessor;
            this.enableAuthSnippet = serviceOptions.Value.EnableAuthenticationTrackingJavaScript;
        }

        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var doNotTrack = false;
            if (context.HttpContext.Request.Headers.TryGetValue("DNT", out var doNotTrackHeaderValue))
            {
                doNotTrack = string.Equals(doNotTrackHeaderValue, "1");
            }

            if (!telemetryConfiguration.DisableTelemetry && 
                !doNotTrack && 
                !string.IsNullOrEmpty(telemetryConfiguration.InstrumentationKey))
            {
                string additionalJS = string.Empty;
                IIdentity identity = httpContextAccessor?.HttpContext.User?.Identity;
                if (enableAuthSnippet &&
                    identity != null &&
                    identity.IsAuthenticated)
                {
                    string escapedUserName = JsonConvert.ToString(identity.Name);
                    additionalJS = string.Format(CultureInfo.InvariantCulture, AuthSnippet, escapedUserName);
                }

                var html = string.Format(CultureInfo.InvariantCulture, Snippet, telemetryConfiguration.InstrumentationKey, additionalJS);
                writer.WriteUnencodedText(html);
            }

            base.RenderControl(writer, context);
        }
    }
}
