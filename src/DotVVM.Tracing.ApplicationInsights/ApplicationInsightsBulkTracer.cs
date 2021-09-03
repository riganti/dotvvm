using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Utils;
using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace DotVVM.Tracing.ApplicationInsights
{
    public class ApplicationInsightsBulkTracer : IRequestTracer
    {
        private readonly TelemetryClient telemetryClient;
        private readonly Stopwatch stopwatch = Stopwatch.StartNew();

        private List<Event> events = new List<Event>();

        public ApplicationInsightsBulkTracer(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        public Task EndRequest(IDotvvmRequestContext context)
        {
            foreach (var @event in events)
            {
                telemetryClient.TrackMetric(@event.Name, @event.TimeStamp);
            }

            return TaskUtils.GetCompletedTask();
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            telemetryClient.TrackException(exception);

            return EndRequest(context);
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            var newEvent = new Event
            {
                Name = eventName,
                TimeStamp = stopwatch.ElapsedMilliseconds
            };

            events.Add(newEvent);

            return TaskUtils.GetCompletedTask();
        }

        internal struct Event
        {
            public string Name { get; set; }
            public long TimeStamp { get; set; }
        }
    }
}
