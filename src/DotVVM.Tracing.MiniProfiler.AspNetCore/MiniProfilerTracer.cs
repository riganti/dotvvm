using System.Threading.Tasks;
using StackExchange.Profiling;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System;

namespace DotVVM.Tracing.MiniProfiler.AspNetCore
{
    public class MiniProfilerTracer : IRequestTracer
    {
        private const string endRequestEventName = "RequestEnded";
        private Timing currentTiming;

        public Task EndRequest(IDotvvmRequestContext context)
        {
            SetEventNameStopCurrentTiming(endRequestEventName);

            return TaskUtils.GetCompletedTask();
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            SetEventNameStopCurrentTiming(endRequestEventName);

            return TaskUtils.GetCompletedTask();
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            SetEventNameStopCurrentTiming(eventName);

            currentTiming = StackExchange.Profiling.MiniProfiler.Current.Step(string.Empty);
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
