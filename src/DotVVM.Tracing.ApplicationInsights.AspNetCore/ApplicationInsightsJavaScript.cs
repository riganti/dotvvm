using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Tracing.ApplicationInsights.AspNetCore
{
    /// <summary>
    /// This class helps to inject Application Insights JavaScript snippet into dothtml.
    /// </summary>
    public class ApplicationInsightsJavascript : DotvvmControl
    {
        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var doNotTrack = false;
            if (context.HttpContext.Request.Headers.TryGetValue("DNT", out var doNotTrackHeaderValue))
            {
                doNotTrack = string.Equals(doNotTrackHeaderValue, "1");
            }

            if (!doNotTrack)
            {
                var javascriptSnippet = context.Services.GetRequiredService<JavaScriptSnippet>();

                writer.WriteUnencodedText(javascriptSnippet.FullScript);
            }

            base.RenderControl(writer, context);
        }
    }
}
