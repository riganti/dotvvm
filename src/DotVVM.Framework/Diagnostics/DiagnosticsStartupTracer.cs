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
        private Timer timer;
        private readonly object locker = new object();

        private readonly IList<EventTiming> events = new List<EventTiming>();
        private bool startupCompleted = false;
        private int sentEventsCount = 0;

        private long ElapsedMillisSinceLastLog => events.Sum(e => e.Duration);


        public void TraceEvent(string eventName)
        {
            if (eventName == StartupTracingConstants.AddDotvvmStarted)
            {
                stopwatch.Start();
            }

            var eventTiming = CreateEventTiming(eventName);
            lock (locker)
            {
                events.Add(eventTiming);
            }
        }

        public Task NotifyStartupCompleted(IDiagnosticsInformationSender informationSender)
        {
            if (startupCompleted)
            {
                throw new InvalidOperationException($"{nameof(NotifyStartupCompleted)} cannot be called twice!");
            }
            startupCompleted = true;

            // create a timer to report after-startup events regularly
            this.timer = new Timer(state =>
            {
                lock (locker)
                {
                    if (sentEventsCount < events.Count)
                    {
                        informationSender.SendInformationAsync(BuildDiagnosticsInformation(events.Skip(sentEventsCount).ToList()));
                        sentEventsCount = events.Count;
                    }
                }
            }, null, 10000, 10000);

            // report startup events
            DiagnosticsInformation info;
            lock (locker)
            {
                info = BuildDiagnosticsInformation(events);
                sentEventsCount = events.Count;
            }
            return informationSender.SendInformationAsync(info);
        }

        private EventTiming CreateEventTiming(string eventName)
        {
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            return new EventTiming
            {
                Duration = elapsedMilliseconds - ElapsedMillisSinceLastLog,
                TotalDuration = elapsedMilliseconds,
                EventName = eventName
            };
        }

        private DiagnosticsInformation BuildDiagnosticsInformation(IList<EventTiming> eventTimings)
        {
            return new DiagnosticsInformation()
            {
                RequestDiagnostics = new RequestDiagnostics()
                {
                    Url = "{APPLICATION_STARTUP}"
                },
                ResponseDiagnostics = new ResponseDiagnostics(),
                EventTimings = eventTimings,
                TotalDuration = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
