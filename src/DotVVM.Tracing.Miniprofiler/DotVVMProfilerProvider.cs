using System.Threading.Tasks;
using StackExchange.Profiling;

namespace DotVVM.Tracing.MiniProfiler
{
    public class DotVVMProfilerProvider : IAsyncProfilerProvider
    {
        private IAsyncProfilerProvider profileProvider;
        public DotVVMProfilerProvider(IAsyncProfilerProvider profileProvider)
        {
            this.profileProvider = profileProvider;
        }

        public StackExchange.Profiling.MiniProfiler GetCurrentProfiler()
        {
            return profileProvider.GetCurrentProfiler();
        }

        public StackExchange.Profiling.MiniProfiler Start(string profilerName = null)
        {
            return profileProvider.Start(profilerName);
        }

        public void Stop(bool discardResults)
        {
            if (!discardResults)
            {
                discardResults = EnsureName();
            }

            profileProvider.Stop(discardResults);
        }

        public Task StopAsync(bool discardResults)
        {
            if (!discardResults)
            {
                discardResults = EnsureName();
            }

            return profileProvider.StopAsync(discardResults);
        }

        private bool EnsureName()
        {
            var currentMiniProfiler = GetCurrentProfiler();

            // Exclude all dotvvm Resources - others have some name
            return string.IsNullOrEmpty(currentMiniProfiler.Name);
        }
    }
}
