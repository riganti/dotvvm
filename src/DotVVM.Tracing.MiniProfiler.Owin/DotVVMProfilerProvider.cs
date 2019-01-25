using System.Threading.Tasks;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;

namespace DotVVM.Tracing.MiniProfiler.Owin
{
    public class DotVVMProfilerProvider : IAsyncProfilerProvider
    {
        private IAsyncProfilerProvider provider;

        public DotVVMProfilerProvider(IAsyncProfilerProvider provider)
        {
            this.provider = provider;
        }

        public StackExchange.Profiling.MiniProfiler GetCurrentProfiler()
        {
            return provider.CurrentProfiler;
        }

        private bool EnsureName()
        {
            var currentMiniProfiler = GetCurrentProfiler();

            // Exclude all dotvvm Resources - others have some name
            return string.IsNullOrEmpty(currentMiniProfiler?.Name);
        }

        public StackExchange.Profiling.MiniProfiler Start(string profilerName, MiniProfilerBaseOptions options)
        {
            return provider.Start(profilerName, options);
        }

        public void Stopped(StackExchange.Profiling.MiniProfiler profiler, bool discardResults)
        {
            provider.Stopped(profiler, discardResults);
        }

        public Task StoppedAsync(StackExchange.Profiling.MiniProfiler profiler, bool discardResults)
        {
            return provider.StoppedAsync(profiler, discardResults);
        }

        public StackExchange.Profiling.MiniProfiler CurrentProfiler => this.provider.CurrentProfiler;
    }
}