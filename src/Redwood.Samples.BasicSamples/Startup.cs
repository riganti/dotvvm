using System.Web.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Redwood.Framework;
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
            RedwoodConfiguration redwoodConfiguration;
            app.UseRedwood(applicationPhysicalPath, out redwoodConfiguration);

            redwoodConfiguration.ResourceRepo.Register("jquery", new ScriptResource("Scripts/jquery-2.1.1.min.js"));
            redwoodConfiguration.ResourceRepo.Register("knockout-core", new ScriptResource("/Scripts/knockout-3.2.0.js", "jquery"));
            redwoodConfiguration.ResourceRepo.Register("knockout", new ScriptResource("/Scripts/knockout.mapper.js", "knockout-core"));
            redwoodConfiguration.ResourceRepo.Register("redwood", new ScriptResource("/Scripts/Redwood.js", "knockout"));
            redwoodConfiguration.ResourceRepo.Register("bootstrap-css", new StyleResource("Content/bootstrap/bootstrap.min.css"));
            redwoodConfiguration.ResourceRepo.Register("bootstrap", new ScriptResource("Scripts/bootstrap.min.js", new string[] { "bootstrap-css", "jquery" }));

            redwoodConfiguration.RouteTable.Add("Sample1", "Sample1", "sample1.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample2", "Sample2", "sample2.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample3", "Sample3", "sample3.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample4", "Sample4", "sample4.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample5", "Sample5", "sample5.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample6", "Sample6", "sample6.rwhtml", null);

            // use static files
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = new PhysicalFileSystem(applicationPhysicalPath)
            });
        }
    }
}
