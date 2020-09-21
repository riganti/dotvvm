using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using StackExchange.Profiling;
using System.Threading.Tasks;

namespace DotVVM.Tracing.MiniProfiler
{
    public interface IMiniProfilerRequestTracer
    {
        Timing Step(string name); 
        Timing StepIf(string name, long minDuration, bool includeChildren = false);
    }
}