using System.Web.Hosting;
using Microsoft.Owin;
using Owin;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling;

[assembly: OwinStartup(typeof(DotVVM.Samples.MiniProfiler.Owin.Startup))]
namespace DotVVM.Samples.MiniProfiler.Owin
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            InitProfilerSettings();

            // use DotVVM
            var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath, options: options =>
            {
                options
                    .AddDefaultTempStorages("temp")
                    .AddMiniProfilerEventTracing();
            });
#if !DEBUG
            dotvvmConfiguration.Debug = false;
#endif
        }
    }
}
