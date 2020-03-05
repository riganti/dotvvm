using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics.Models;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Diagnostics
{
    public class DiagnosticsStartupTracer : IStartupTracer 
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly IList<EventTiming> events = new List<EventTiming>();

        private long ElapsedMillisSinceLastLog => events.Sum(e => e.Duration);

        public void TraceEvent(string eventName)
        {
            if (eventName == StartupTracingConstants.AddDotvvmStarted)
            {
                stopwatch.Start();
            }
            events.Add(CreateEventTiming(eventName));
        }

        public Task NotifyStartupCompleted(IDiagnosticsInformationSender informationSender)
        {
            return informationSender.SendInformationAsync(new DiagnosticsInformation()
            {
                RequestDiagnostics = new RequestDiagnostics() {
                    Url = "{APPLICATION_STARTUP}"
                },
                ResponseDiagnostics = new ResponseDiagnostics(),
                EventTimings = events,
                TotalDuration = stopwatch.ElapsedMilliseconds
            });
        }

        private EventTiming CreateEventTiming(string eventName)
        {
            return new EventTiming {
                Duration = stopwatch.ElapsedMilliseconds - ElapsedMillisSinceLastLog,
                EventName = eventName
            };
        }
    }
}
