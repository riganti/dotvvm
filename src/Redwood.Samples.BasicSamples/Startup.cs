using System;
using System.Web.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Redwood.Framework.Configuration;
using Redwood.Framework.Hosting;
using Redwood.Framework.Controls;

[assembly: OwinStartup(typeof(Redwood.Samples.BasicSamples.Startup))]
namespace Redwood.Samples.BasicSamples
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            // use Redwood
            var redwoodConfiguration = new RedwoodConfiguration()
            {
                ApplicationPhysicalPath = applicationPhysicalPath
            };
            redwoodConfiguration.ResourceRepo.Register("knockout-core", new ScriptResource("/Scripts/knockout-3.2.0.js"));
            redwoodConfiguration.ResourceRepo.Register("knockout", new ScriptResource("/Scripts/knockout.mapping-latest.js", "knockout-core"));
            redwoodConfiguration.ResourceRepo.Register("redwood", new ScriptResource("/Scripts/Redwood.js", "knockout"));

            redwoodConfiguration.RouteTable.Add("Sample1", "Sample1", "sample1.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample2", "Sample2", "sample2.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample3", "Sample3", "sample3.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample4", "Sample4", "sample4.rwhtml", null);
            app.Use<RedwoodMiddleware>(redwoodConfiguration);

            // use static files
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = new PhysicalFileSystem(applicationPhysicalPath)
            });
        }
    }
}
