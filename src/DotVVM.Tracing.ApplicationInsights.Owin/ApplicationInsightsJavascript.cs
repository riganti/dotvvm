using System.Globalization;
using System.Security.Principal;
using System.Text.Encodings.Web;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Microsoft.ApplicationInsights.Extensibility;

namespace DotVVM.Tracing.ApplicationInsights.Owin
{
    /// <summary>
    /// This class helps to inject Application Insights JavaScript snippet into dothtml.
    /// </summary>
    public class ApplicationInsightsJavascript : DotvvmControl
    {
        /// <summary>JavaScript snippet.</summary>
        private static readonly string Snippet = Resources.JavaScriptSnippet;

        /// <summary>JavaScript authenticated user tracking snippet.</summary>
        private static readonly string AuthSnippet = Resources.JavaScriptAuthSnippet;

        /// <summary>Configuration instance.</summary>
        private TelemetryConfiguration telemetryConfiguration;

        /// <summary>JavaScript encoder.</summary>
        private JavaScriptEncoder encoder;

        /// <summary>
        /// Enables tracking of authenticated user tracking by their usernames. 
        /// See https://github.com/Microsoft/ApplicationInsights-JS/blob/master/API-reference.md#setauthenticatedusercontext
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool EnableAuthSnippet
        {
            get { return (bool)GetValue(EnableAuthSnippetProperty); }
            set { SetValue(EnableAuthSnippetProperty, value); }
        }
        public static readonly DotvvmProperty EnableAuthSnippetProperty
            = DotvvmProperty.Register<bool, ApplicationInsightsJavascript>(c => c.EnableAuthSnippet, false);

        /// <summary>
        /// Initializes a new instance of the ApplicationInsightsJavascript class.
        /// </summary>
        public ApplicationInsightsJavascript()
        {
            telemetryConfiguration = TelemetryConfiguration.Active;
            encoder = JavaScriptEncoder.Default;
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
                writer.WriteUnencodedText(RenderSnippet(context.HttpContext));
            }

            base.RenderControl(writer, context);
        }

        private string RenderSnippet(IHttpContext httpContext)
        {
            if (!telemetryConfiguration.DisableTelemetry && !string.IsNullOrEmpty(telemetryConfiguration.InstrumentationKey))
            {
                string additionalJS = string.Empty;
                IIdentity identity = httpContext.User?.Identity;
                if (EnableAuthSnippet &&
                    identity != null &&
                    identity.IsAuthenticated)
                {
                    string escapedUserName = encoder.Encode(identity.Name);
                    additionalJS = string.Format(CultureInfo.InvariantCulture, AuthSnippet, escapedUserName);
                }
                return string.Format(CultureInfo.InvariantCulture, Snippet, telemetryConfiguration.InstrumentationKey, additionalJS);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
