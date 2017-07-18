using System.Threading.Tasks;
using StackExchange.Profiling;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Tracing.MiniProfiler
{
    public class MiniProfilerTracer : IRequestTracer
    {
        private Timing currentTiming;

        public Task EndRequest(IDotvvmRequestContext context)
        {
            return TaskUtils.GetCompletedTask();
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            if (currentTiming != null)
            {
                currentTiming.Name = eventName;
                currentTiming.Stop();
            }

            currentTiming = StackExchange.Profiling.MiniProfiler.Current.Step(string.Empty);
            return TaskUtils.GetCompletedTask();
        }
    }
}
