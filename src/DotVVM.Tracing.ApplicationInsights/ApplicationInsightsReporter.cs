using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Utils;
using Microsoft.ApplicationInsights;

namespace DotVVM.Tracing.ApplicationInsights
{
    public class ApplicationInsightsReporter : IRequestTracingReporter
    {
        private readonly TelemetryClient telemetryClient;

        public ApplicationInsightsReporter(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        public Task TraceMetrics(Dictionary<string, object> traceData)
        {
            try
            {
                if (traceData == null)
                {
                    return TaskUtils.GetCompletedTask();
                }
                foreach (string eventName in traceData.Keys)
                {
                    if (traceData[eventName] != null)
                    {
                        telemetryClient.TrackMetric(eventName, double.Parse(traceData[eventName].ToString()));
                    }
                }
            }
            catch (Exception) { }
            return TaskUtils.GetCompletedTask();
        }

        public Task TraceException(Exception exception)
        {
            telemetryClient.TrackException(exception);
            return TaskUtils.GetCompletedTask();
        }
    }
}
