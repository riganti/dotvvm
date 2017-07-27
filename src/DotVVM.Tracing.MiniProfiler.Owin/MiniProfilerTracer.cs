using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System;
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
            StackExchange.Profiling.MiniProfiler.Stop();

            return TaskUtils.GetCompletedTask();
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            SetEventNameStopCurrentTiming(endRequestEventName);
            StackExchange.Profiling.MiniProfiler.Stop();

            return TaskUtils.GetCompletedTask();
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            if (StackExchange.Profiling.MiniProfiler.Current == null)
            {
                StackExchange.Profiling.MiniProfiler.Start();
            }

            SetEventNameStopCurrentTiming(eventName);

            currentTiming = (Timing)StackExchange.Profiling.MiniProfiler.Current.Step(string.Empty);
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
