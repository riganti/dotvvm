using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using Microsoft.ApplicationInsights;

namespace DotVVM.Tracing.ApplicationInsights
{
    public class ApplicationInsightExceptionFilter : ActionFilterAttribute
    {
        private readonly TelemetryClient telemetryClient;

        public ApplicationInsightExceptionFilter(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        protected override Task OnPageExceptionAsync(IDotvvmRequestContext context, Exception exception)
        {
            telemetryClient.TrackException(exception);

            return base.OnPageExceptionAsync(context, exception);
        }
    }
}
