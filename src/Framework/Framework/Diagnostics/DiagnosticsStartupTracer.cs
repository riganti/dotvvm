using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        // list with concurrent read capability
        private ImmutableList<EventTiming> events = ImmutableList<EventTiming>.Empty;
        private int startupCompleted;

        private long ElapsedMillisecondsSinceLastLog => events.Sum(e => e.Duration);

        private event Func<DiagnosticsInformation, Task>? LateInfoReported;

        public void TraceEvent(string eventName)
        {
            if (eventName == StartupTracingConstants.AddDotvvmStarted)
            {
                stopwatch.Start();
            }

            var eventTiming = CreateEventTiming(eventName);
            ConcurrencyUtils.CasChange(ref events, l => l.Add(eventTiming));
            var reportLateEvent = startupCompleted > 0;

            if (reportLateEvent)
            {
                LateInfoReported?.Invoke(BuildDiagnosticsInformation(ImmutableList.Create(eventTiming), stopwatch.ElapsedMilliseconds));
            }
        }

        public Task NotifyStartupCompleted(IDiagnosticsInformationSender informationSender)
        {
            DiagnosticsInformation info;
            var competedCount = Interlocked.Increment(ref startupCompleted);
            if (competedCount > 1)
            {
                throw new InvalidOperationException($"{nameof(NotifyStartupCompleted)} cannot be called twice!");
            }

            LateInfoReported += i => informationSender.SendInformationAsync(i);

            info = BuildDiagnosticsInformation(events, stopwatch.ElapsedMilliseconds);

            // report startup events
            return informationSender.SendInformationAsync(info);
        }

        private EventTiming CreateEventTiming(string eventName)
        {
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            return new EventTiming(
                eventName,
                elapsedMilliseconds - ElapsedMillisecondsSinceLastLog,
                elapsedMilliseconds
            );
        }

        private static DiagnosticsInformation BuildDiagnosticsInformation(ImmutableList<EventTiming> eventTimings, long elapsedMilliseconds)
        {
            return new DiagnosticsInformation(
                new RequestDiagnostics(
                    RequestType.Init,
                    "",
                    "{APPLICATION_STARTUP}",
                    Enumerable.Empty<HttpHeaderItem>(),
                    null
                ),
                new ResponseDiagnostics(),
                eventTimings,
                elapsedMilliseconds
            );
        }
    }
}
