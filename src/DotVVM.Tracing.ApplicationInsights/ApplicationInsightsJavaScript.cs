using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Microsoft.ApplicationInsights.AspNetCore;

namespace DotVVM.Tracing.ApplicationInsights
{
    /// <summary>
    /// This class helps to inject Application Insights JavaScript snippet into dothtml.
    /// </summary>
    public class ApplicationInsightJavascript : DotvvmControl
    {
        /// <summary>JavaScriptSnippet instance.</summary>
        private JavaScriptSnippet javascriptSnippet;

        /// <summary>
        /// Initializes a new instance of the ApplicationInsightJavascript class.
        /// </summary>
        /// <param name="javascriptSnippet">Helper class to inject Application Insight snippet to application code.</param>
        public ApplicationInsightJavascript(JavaScriptSnippet javascriptSnippet)
        {
            this.javascriptSnippet = javascriptSnippet;
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
                writer.WriteUnencodedText(javascriptSnippet.FullScript);
            }

            base.RenderControl(writer, context);
        }
    }
}
