using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Utils;
using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace DotVVM.Tracing.ApplicationInsights
{
    public class ApplicationInsightsTracer : IRequestTracer
    {
        private readonly TelemetryClient telemetryClient;

        private Stopwatch stopwatch = Stopwatch.StartNew();

        public ApplicationInsightsTracer(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            telemetryClient.TrackMetric(eventName, stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            return TaskUtils.GetCompletedTask();
        }

        public Task EndRequest(IDotvvmRequestContext context)
        {
            return TaskUtils.GetCompletedTask();
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            telemetryClient.TrackException(exception);

            return TaskUtils.GetCompletedTask();
        }
    }
}
