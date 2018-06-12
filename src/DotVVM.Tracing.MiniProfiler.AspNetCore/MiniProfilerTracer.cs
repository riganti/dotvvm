using System.Threading.Tasks;
using StackExchange.Profiling;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System;
using System.Linq;

namespace DotVVM.Tracing.MiniProfiler.AspNetCore
{
    public class MiniProfilerTracer : IRequestTracer
    {
        private const string endRequestEventName = "RequestEnded";
        private Timing currentTiming;

        public Task EndRequest(IDotvvmRequestContext context)
        {
            SetEventNameStopCurrentTiming(context, endRequestEventName);

            return TaskUtils.GetCompletedTask();
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            SetEventNameStopCurrentTiming(context, endRequestEventName);

            return TaskUtils.GetCompletedTask();
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            SetEventNameStopCurrentTiming(context, eventName);

            var mp = StackExchange.Profiling.MiniProfiler.Current;

            if (mp != null)
            {
                currentTiming = mp.Step(eventName);
            }

            return TaskUtils.GetCompletedTask();
        }

        private void SetEventNameStopCurrentTiming(IDotvvmRequestContext context, string eventName)
        {
            if (currentTiming != null)
            {
                var mp = StackExchange.Profiling.MiniProfiler.Current;
                foreach (var child in mp.Root.Children.Where(c => c != currentTiming && c.StartMilliseconds >= currentTiming.StartMilliseconds).ToArray())
                {
                    mp.Root.Children.Remove(child);
                    currentTiming.AddChild(child);
                }

                currentTiming.Name = eventName;
                currentTiming.Stop();
                currentTiming = null;
            }
        }
    }
}
