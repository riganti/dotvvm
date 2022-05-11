using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.Extensions.Logging;

namespace DotVVM.Framework.Diagnostics
{
    /// <summary> DotVVM request tracer which produces log warning when a request is very slow or when view model is too large. </summary>
    public class PerformanceWarningTracer : IRequestTracer
    {
        private readonly RuntimeWarningCollector logger;
        private readonly DotvvmPerfWarningsConfiguration config;
        private readonly JsonSizeAnalyzer jsonSizeAnalyzer;
        private readonly Stopwatch stopwatch = Stopwatch.StartNew();
        private readonly List<(string eventName, TimeSpan timestamp)> events = new();
        public PerformanceWarningTracer(DotvvmConfiguration config, RuntimeWarningCollector logger, JsonSizeAnalyzer jsonSizeAnalyzer)
        {
            this.logger = logger;
            this.config = config.Diagnostics.PerfWarnings;
            this.jsonSizeAnalyzer = jsonSizeAnalyzer;
        }
        void WarnSlowRequest(TimeSpan totalElapsed)
        {
            var eventTimes = new List<(string eventName, TimeSpan time)>();
            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                var time = i > 0 ? e.timestamp - events[i - 1].timestamp : e.timestamp;
                eventTimes.Add((e.eventName, time));
            }

            eventTimes.Sort((a, b) => b.time.CompareTo(a.time));

            // take top 4 events or the events which took 90% of total time (whichever is smaller)
            var topEvents = new List<(string eventName, TimeSpan time)>();
            foreach (var e in eventTimes)
            {
                if (topEvents.Count >= 4 || topEvents.Sum(e => e.time.TotalSeconds) > totalElapsed.TotalSeconds * 0.90)
                    break;
                topEvents.Add(e);
            }

            var eventsMsg = string.Join(", ", topEvents.Select(e => $"{e.eventName} ({e.time.TotalSeconds:0.00}s)"));
            logger.Warn(new DotvvmRuntimeWarning(
                $"The request took {totalElapsed.TotalSeconds:0.0}s to process, {(topEvents.Count == 1 ? "slowest event" : topEvents.Count + " slowest events")}: {eventsMsg}.\n" +
                "We recommend using MiniProfiler when to keep an eye on runtime performance: https://www.dotvvm.com/docs/latest/pages/concepts/diagnostics-and-profiling/miniprofiler"
            ));
        }
        void WarnLargeViewModel(long viewModelSize, IDotvvmRequestContext context)
        {
            if (context.ViewModelJson is null)
                return;

            var vmAnalysis = jsonSizeAnalyzer.Analyze(context.ViewModelJson);

            var topClasses =
                vmAnalysis.Classes
                .OrderByDescending(c => c.Value.Size.ExclusiveSize)
                .Take(3)
                // only classes which have at least 5% impact
                .Where(c => c.Value.Size.ExclusiveSize > vmAnalysis.TotalSize / 20)
                .ToArray();
            var topProperties =
                vmAnalysis.Classes
                .SelectMany(c => c.Value.Properties.Select(p => (Key: c.Key + "." + p.Key, p.Value)))
                .OrderByDescending(c => c.Value.ExclusiveSize)
                .Take(3)
                // only properties which have at least 5% impact
                .Where(c => c.Value.ExclusiveSize > vmAnalysis.TotalSize / 20)
                .ToArray();

            var byteToPercent = 100.0 / vmAnalysis.TotalSize;

            var msg = $"The serialized view model has {viewModelSize / 1024.0 / 1024.0:0.0}MB, which may make your application quite slow. " +
                string.Join(", ",
                    topProperties.Select(c => $"Property {c.Key} takes {c.Value.ExclusiveSize * byteToPercent:0}%").Concat(
                    topClasses.Select(c => $"Class {c.Key} takes {c.Value.Size.ExclusiveSize * byteToPercent:0}%")));

            logger.Warn(new DotvvmRuntimeWarning(
                msg
            ));
        }
        public Task EndRequest(IDotvvmRequestContext context)
        {
            var elapsed = stopwatch.Elapsed;
            if (elapsed.TotalSeconds > config.SlowRequestSeconds)
                WarnSlowRequest(elapsed);


            var viewModelSize = context.HttpContext.GetItem<int>("dotvvm-viewmodel-size-bytes");
            if (viewModelSize > config.BigViewModelBytes)
            {
                WarnLargeViewModel(viewModelSize, context);
            }

            return Task.CompletedTask;
        }
        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            // you've already got an exception, no need for perf warnings
            return Task.CompletedTask;
        }
        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            events.Add((eventName, stopwatch.Elapsed));
            return Task.CompletedTask;
        }
    }
}
