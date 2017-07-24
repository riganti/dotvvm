using System.Web.Hosting;
using ApplicationInsights.OwinExtensions;
using Microsoft.Owin;
using Owin;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Tracing.ApplicationInsights.Owin;

[assembly: OwinStartup(typeof(DotVVM.Samples.ApplicationInsingts.Owin.Startup))]
namespace DotVVM.Samples.ApplicationInsingts.Owin
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseApplicationInsights();

            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            // use DotVVM
            var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath, options: options =>
            {
                options.AddDefaultTempStorages("temp");
                options.AddApplicationInsightsTracing();
            });
#if !DEBUG
            dotvvmConfiguration.Debug = false;
#endif
        }
    }
}
