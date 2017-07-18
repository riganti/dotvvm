using DotVVM.Framework.Configuration;
using StackExchange.Profiling;

namespace DotVVM.Tracing.MiniProfiler
{
    public static class MiniProfilerBuilderExtensions
    {
        /// <summary>
        /// Registers MiniProfiler tracer and MiniProfilerWidget
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static DotvvmConfiguration AddMiniProfilerEventTracing(this DotvvmConfiguration config, string controlPrefix = "mp")
        {
            config.Runtime.GlobalFilters.Add(new MiniProfilerActionFilter());
            config.Markup.AddCodeControls(controlPrefix, typeof(MiniProfilerWidget));

            config.Runtime.TracerFactories.Add(() => new MiniProfilerTracer());

            var currentProfiler = StackExchange.Profiling.MiniProfiler.Settings.ProfilerProvider
                ?? new DefaultProfilerProvider();

            StackExchange.Profiling.MiniProfiler.Settings.ProfilerProvider = new DotVVMProfilerProvider(currentProfiler);

            return config;
        }
    }
}
