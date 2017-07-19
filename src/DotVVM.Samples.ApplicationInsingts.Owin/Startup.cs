using System.Web.Hosting;
using ApplicationInsights.OwinExtensions;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using DotVVM.Framework;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Tracing.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

[assembly: OwinStartup(typeof(DotVVM.Samples.ApplicationInsingts.Owin.Startup))]
namespace DotVVM.Samples.ApplicationInsingts.Owin
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseApplicationInsights();

            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            //var builder = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;
            //builder.Use((next) => new RequestFilter(next));
            //builder.Build();

            // use DotVVM

            var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath, options: options =>
            {
                options.AddDefaultTempStorages("temp");
                options.AddApplicationInsightsTracing();
            });
#if !DEBUG
            dotvvmConfiguration.Debug = false;
#endif

            // use static files
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = new PhysicalFileSystem(applicationPhysicalPath)
            });
        }
    }
}
