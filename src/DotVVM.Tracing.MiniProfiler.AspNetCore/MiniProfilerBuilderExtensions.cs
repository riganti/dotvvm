using System;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Tracing.MiniProfiler.AspNetCore;
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

            services.Configure((MiniProfilerOptions opt) =>
            {
                opt.IgnoredPaths.Add("/dotvvmResource/");
            });

            services.Configure((DotvvmConfiguration conf) =>
            {
                conf.Markup.AddCodeControls("dot", typeof(MiniProfilerWidget));
                conf.Runtime.GlobalFilters.Add(
                    new MiniProfilerActionFilter(conf.ServiceProvider.GetService<IOptions<MiniProfilerOptions>>()));
            });

            return services;
        }
    }



}
