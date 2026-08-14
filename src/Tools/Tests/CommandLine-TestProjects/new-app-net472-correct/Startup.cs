using System;
using System.Web.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using DotVVM.Framework;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: OwinStartup(typeof(new_app_net472_correct.Startup))]
namespace new_app_net472_correct
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            ConfigureAuth(app);

            // use DotVVM
            var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath, debug: IsInDebugMode());
            dotvvmConfiguration.AssertConfigurationIsValid();

            // use static files
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = new PhysicalFileSystem(applicationPhysicalPath)
            });

            if (IsInDebugMode()) 
            {
                app.UseDotvvmHotReload();
            }
        }
        public void ConfigureAuth(IAppBuilder app)
        {
        }

		private bool IsInDebugMode()
        {
#if !DEBUG
			return false;
#endif
            return true;
        }
    }
}
