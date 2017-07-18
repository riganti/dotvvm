using System;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Profiling;

namespace DotVVM.Tracing.MiniProfiler
{
    public static class MiniProfilerBuilderExtensions
    {
        /// <summary>
        /// Registers MiniProfiler tracer and MiniProfilerWidget
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IDotvvmOptions AddMiniProfilerEventTracing(this IDotvvmOptions options)
        {
            options.Services.AddTransient<IConfigureOptions<DotvvmConfiguration>, MiniProfilerSetup>();

            return options;
        }
    }

    internal class MiniProfilerSetup : IConfigureOptions<DotvvmConfiguration>
    {
        public void Configure(DotvvmConfiguration config)
        {
            config.Markup.AddCodeControls("dot", typeof(MiniProfilerWidget));
            config.Runtime.TracerFactories.Add(() => new MiniProfilerTracer());
            config.Runtime.GlobalFilters.Add(new MiniProfilerActionFilter());

            var currentProfiler = StackExchange.Profiling.MiniProfiler.Settings.ProfilerProvider
                ?? new DefaultProfilerProvider();

            StackExchange.Profiling.MiniProfiler.Settings.ProfilerProvider = new DotVVMProfilerProvider(currentProfiler);
        }
    }
}
