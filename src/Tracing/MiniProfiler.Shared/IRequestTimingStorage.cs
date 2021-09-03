using StackExchange.Profiling;

namespace DotVVM.Tracing.MiniProfiler
{
    public interface IRequestTimingStorage
    {
        Timing Current { get; set; }
    }
}