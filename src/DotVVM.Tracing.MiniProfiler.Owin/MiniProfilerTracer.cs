using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling;

namespace DotVVM.Tracing.MiniProfiler.Owin
{
    public class MiniProfilerTracer : IRequestTracer
    {
        private const string endRequestEventName = "RequestEnded";
        private Timing currentTiming;

        public Task EndRequest(IDotvvmRequestContext context)
        {
            SetEventNameStopCurrentTiming(endRequestEventName);
            return StopProfiler();
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            SetEventNameStopCurrentTiming(endRequestEventName);
            return StopProfiler();
        }

        private Task StopProfiler()
        {
            return GetProfilerCurrent()?.StopAsync() ?? TaskUtils.GetCompletedTask();
        }

        private StackExchange.Profiling.MiniProfiler GetProfilerCurrent()
        {
            return StackExchange.Profiling.MiniProfiler.Current;
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            if (GetProfilerCurrent() == null)
            {
                StackExchange.Profiling.MiniProfiler.StartNew();
            }
            SetEventNameStopCurrentTiming(eventName);

            currentTiming = (Timing)GetProfilerCurrent().Step(string.Empty);
            return TaskUtils.GetCompletedTask();
        }

        private void SetEventNameStopCurrentTiming(string eventName)
        {
            if (currentTiming != null)
            {
                currentTiming.Name = eventName;
                currentTiming.Stop();
            }
        }
    }
}