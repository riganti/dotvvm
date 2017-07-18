using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Utils;
using Microsoft.ApplicationInsights;

namespace DotVVM.Tracing.ApplicationInsights
{
    public class ApplicationInsightsTracer : IRequestTracer
    {
        private readonly TelemetryClient telemetryClient;
        private readonly IStopwatch stopwatch;

        private long? lastStopwatchTime;

        public ApplicationInsightsTracer(TelemetryClient telemetryClient, IStopwatch stopwatch)
        {
            this.telemetryClient = telemetryClient;
            this.stopwatch = stopwatch;
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            if (lastStopwatchTime.HasValue)
            {
                var duration = stopwatch.GetElapsedMiliseconds() - lastStopwatchTime.Value;
                telemetryClient.TrackMetric(eventName, duration);
            }

            lastStopwatchTime = stopwatch.GetElapsedMiliseconds();

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
