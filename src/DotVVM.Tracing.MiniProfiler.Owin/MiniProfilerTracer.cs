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
        private Timing currentTiming;
        private bool requestStarted;

        public Task EndRequest(IDotvvmRequestContext context)
        {
            StackExchange.Profiling.MiniProfiler.Stop();

            return TaskUtils.GetCompletedTask();
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            StackExchange.Profiling.MiniProfiler.Stop();

            return TaskUtils.GetCompletedTask();
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            if (StackExchange.Profiling.MiniProfiler.Current == null)
            {
                StackExchange.Profiling.MiniProfiler.Start();
            }

            if (currentTiming != null)
            {
                currentTiming.Name = eventName;
                currentTiming.Stop();
            }

            currentTiming = (Timing)StackExchange.Profiling.MiniProfiler.Current.Step(string.Empty);
            return TaskUtils.GetCompletedTask();
        }
    }
}
