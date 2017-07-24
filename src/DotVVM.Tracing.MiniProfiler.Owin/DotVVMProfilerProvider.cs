using StackExchange.Profiling;

namespace DotVVM.Tracing.MiniProfiler.Owin
{
    public class DotVVMProfilerProvider : IProfilerProvider
    {
        private IProfilerProvider profileProvider;

        public DotVVMProfilerProvider(IProfilerProvider profileProvider)
        {
            this.profileProvider = profileProvider;
        }

        public StackExchange.Profiling.MiniProfiler GetCurrentProfiler()
        {
            return profileProvider.GetCurrentProfiler();
        }

        public StackExchange.Profiling.MiniProfiler Start(ProfileLevel level, string sessionName = null)
        {
            return profileProvider.Start(level, sessionName);
        }

        public void Stop(bool discardResults)
        {
            if (!discardResults)
            {
                discardResults = EnsureName();
            }

            profileProvider.Stop(discardResults);
        }

        private bool EnsureName()
        {
            var currentMiniProfiler = GetCurrentProfiler();

            // Exclude all dotvvm Resources - others have some name
            return string.IsNullOrEmpty(currentMiniProfiler?.Name);
        }
    }
}
