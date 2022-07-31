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

        private long ElapsedMillisecondsSinceLastLog => events.Sum(e => e.Duration);

        private event Func<DiagnosticsInformation, Task>? LateInfoReported;

        public void TraceEvent(string eventName)
        {
            if (eventName == StartupTracingConstants.AddDotvvmStarted)
            {
                stopwatch.Start();
            }

            var list = ConcurrencyUtils.CasChange(ref events, l => l.Add(CreateEventTiming(eventName, l)));
            var reportLateEvent = list.Any(x => x.StartupComplete);

            if (reportLateEvent)
            {
                var eventTiming = list.FindLast(_ => true)!;
                LateInfoReported?.Invoke(BuildDiagnosticsInformation(ImmutableList.Create(eventTiming), stopwatch.ElapsedMilliseconds));
            }
        }

        public Task NotifyStartupCompleted(IDiagnosticsInformationSender informationSender)
        {
            var list = ConcurrencyUtils.CasChange(ref events, l => {

                if (l.Any(x => x.StartupComplete))
                {
                    throw new InvalidOperationException($"{nameof(NotifyStartupCompleted)} cannot be called twice!");
                }

                return l.Add(CreateEventTiming(StartupTracingConstants.StartupComplete, l) with { StartupComplete = true });
            });

            LateInfoReported += i => informationSender.SendInformationAsync(i);

            var info = BuildDiagnosticsInformation(list, stopwatch.ElapsedMilliseconds);

            // report startup events
            return informationSender.SendInformationAsync(info);
        }

        private EventTiming CreateEventTiming(string eventName, ImmutableList<EventTiming> existingEvents)
        {
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            return new EventTiming(
                eventName,
                elapsedMilliseconds - (existingEvents.Count > 0 ? existingEvents.FindLast(_ => true)!.TotalDuration : 0),
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
