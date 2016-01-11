using System.Web.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using DotVVM.Framework;
using DotVVM.Framework.Configuration;

[assembly: OwinStartup(typeof($safeprojectname$.Startup))]
namespace $safeprojectname$
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            // use DotVVM
            DotvvmConfiguration dotvvmConfiguration = app.UseDotVVM(applicationPhysicalPath);
#if DEBUG
            dotvvmConfiguration.Debug = true;
#endif
        
        // Routes registration 
        // To register one route by one use: 
        dotvvmConfiguration.RouteTable.Add("Default", "", "Views/default.dothtml", null);

        //OR
        //for automatic registration use IRoutingStrategy
        //dotvvmConfiguration.RouteTable.RegisterRoutingStrategy(new ViewsFolderBasedRouteStrategy(dotvvmConfiguration));


        // use static files
        app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = new PhysicalFileSystem(applicationPhysicalPath)
            });
        }
    }
}
