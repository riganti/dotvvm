using System.Web.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Redwood.Framework;
using Redwood.Framework.Configuration;

[assembly: OwinStartup(typeof($safeprojectname$.Startup))]
namespace $safeprojectname$
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            // use Redwood
            RedwoodConfiguration redwoodConfiguration = app.UseRedwood(applicationPhysicalPath);
            redwoodConfiguration.RouteTable.Add("Default", "", "Views/default.rwhtml", null);

            // use static files
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = new PhysicalFileSystem(applicationPhysicalPath)
            });
        }
    }
}
