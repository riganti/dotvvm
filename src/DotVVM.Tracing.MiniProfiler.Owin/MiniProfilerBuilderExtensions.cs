using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Tracing.MiniProfiler.Owin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Profiling;

namespace DotVVM.Framework.Configuration
{
    public static class MiniProfilerBuilderExtensions
    {
        /// <summary>
        /// Registers MiniProfiler tracer and MiniProfilerWidget
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IDotvvmServiceCollection AddMiniProfilerEventTracing(this IDotvvmServiceCollection services)
        {
            services.AddTransient<IRequestTracer, MiniProfilerTracer>();
            services.AddTransient<IConfigureOptions<DotvvmConfiguration>, MiniProfilerSetup>();

            return services;
        }
    }

    internal class MiniProfilerSetup : IConfigureOptions<DotvvmConfiguration>
    {
        public void Configure(DotvvmConfiguration config)
        {
            config.Markup.AddCodeControls("dot", typeof(MiniProfilerWidget));
            config.Runtime.GlobalFilters.Add(new MiniProfilerActionFilter());

            var currentProfiler = StackExchange.Profiling.MiniProfiler.Settings.ProfilerProvider
                ?? new WebRequestProfilerProvider();

            StackExchange.Profiling.MiniProfiler.Settings.ProfilerProvider = new DotVVMProfilerProvider(currentProfiler);
        }
    }

}
