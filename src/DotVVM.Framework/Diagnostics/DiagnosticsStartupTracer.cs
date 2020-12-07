using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics.Models;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;

namespace DotVVM.Framework.Diagnostics
{
    public class DiagnosticsStartupTracer : IStartupTracer
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly object locker = new object();

        private readonly IList<EventTiming> events = new List<EventTiming>();
        private bool startupCompleted;

        private long ElapsedMillisecondsSinceLastLog => events.Sum(e => e.Duration);

        private event Func<DiagnosticsInformation, Task> LateInfoReported;

        public void TraceEvent(string eventName)
        {
            if (eventName == StartupTracingConstants.AddDotvvmStarted)
            {
                stopwatch.Start();
            }

            var eventTiming = CreateEventTiming(eventName);
            bool reportLateEvent;
            lock (locker)
            {
                events.Add(eventTiming);
                reportLateEvent = startupCompleted;
            }

            if (reportLateEvent)
            {
                LateInfoReported?.Invoke(BuildDiagnosticsInformation(new[] { eventTiming }));
            }
        }

        public Task NotifyStartupCompleted(IDiagnosticsInformationSender informationSender)
        {
            DiagnosticsInformation info;
            lock (locker)
            {
                if (startupCompleted)
                {
                    throw new InvalidOperationException($"{nameof(NotifyStartupCompleted)} cannot be called twice!");
                }
                startupCompleted = true;

                LateInfoReported += i => informationSender.SendInformationAsync(i);

                info = BuildDiagnosticsInformation(events);
            }

            // report startup events
            return informationSender.SendInformationAsync(info);
        }

        private EventTiming CreateEventTiming(string eventName)
        {
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            return new EventTiming {
                Duration = elapsedMilliseconds - ElapsedMillisecondsSinceLastLog,
                TotalDuration = elapsedMilliseconds,
                EventName = eventName
            };
        }

        private DiagnosticsInformation BuildDiagnosticsInformation(IList<EventTiming> eventTimings)
        {
            return new DiagnosticsInformation() {
                RequestDiagnostics = new RequestDiagnostics() {
                    Url = "{APPLICATION_STARTUP}"
                },
                ResponseDiagnostics = new ResponseDiagnostics(),
                EventTimings = eventTimings,
                TotalDuration = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
