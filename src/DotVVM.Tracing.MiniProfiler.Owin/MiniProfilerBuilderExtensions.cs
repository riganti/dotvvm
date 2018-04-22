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
            services.Services.AddTransient<IRequestTracer, MiniProfilerTracer>();
            services.Services.AddTransient<IConfigureOptions<DotvvmConfiguration>, MiniProfilerSetup>();

            return services;
        }
    }

    internal class MiniProfilerSetup : IConfigureOptions<DotvvmConfiguration>
    {
        public void Configure(DotvvmConfiguration options)
        {
            options.Markup.AddCodeControls("dot", typeof(MiniProfilerWidget));
            options.Runtime.GlobalFilters.Add(new MiniProfilerActionFilter());

            var currentProfiler = MiniProfiler.Settings.ProfilerProvider
                ?? new WebRequestProfilerProvider();

            MiniProfiler.Settings.ProfilerProvider = new DotVVMProfilerProvider(currentProfiler);
        }
    }

}
