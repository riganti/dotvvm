using System;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Profiling;

namespace DotVVM.Tracing.MiniProfiler.AspNetCore
{
    public static class MiniProfilerBuilderExtensions
    {
        /// <summary>
        /// Registers MiniProfiler tracer and MiniProfilerWidget
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IDotvvmServiceCollection AddMiniProfilerEventTracing(this IDotvvmServiceCollection options)
        {
            options.AddTransient<IRequestTracer, MiniProfilerTracer>();

            options.Configure((MiniProfilerOptions opt) =>
            {
                opt.IgnoredPaths.Add("/dotvvmResource/");
            });

            options.Configure((DotvvmConfiguration conf) =>
            {
                conf.Markup.AddCodeControls("dot", typeof(MiniProfilerWidget));
                conf.Runtime.GlobalFilters.Add(
                    new MiniProfilerActionFilter(conf.ServiceProvider.GetService<IOptions<MiniProfilerOptions>>()));
            });

            return options;
        }
    }



}
