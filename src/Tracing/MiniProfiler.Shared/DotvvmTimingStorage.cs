using StackExchange.Profiling;

namespace DotVVM.Tracing.MiniProfiler
{
    public class DotvvmTimingStorage : IRequestTimingStorage
    {
        public Timing Current { get; set; }
    }
}