using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Profiling;

namespace DotVVM.Tracing.MiniProfiler.Owin
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
            options.Markup.AddCodeControls(DotvvmConfiguration.DotvvmControlTagPrefix, typeof(MiniProfilerWidget));
            options.Runtime.GlobalFilters.Add(new MiniProfilerActionFilter());

            var currentProfiler = StackExchange.Profiling.MiniProfiler.DefaultOptions.ProfilerProvider ?? new DefaultProfilerProvider();
            StackExchange.Profiling.MiniProfiler.DefaultOptions.ProfilerProvider = new DotVVMProfilerProvider(currentProfiler);
        }
    }
}