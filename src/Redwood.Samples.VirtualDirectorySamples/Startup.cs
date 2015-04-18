using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Redwood.Framework;
using System.Web.Hosting;

[assembly: OwinStartup(typeof(Redwood.Samples.VirtualDirectorySamples.Startup))]

namespace Redwood.Samples.VirtualDirectorySamples
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = app.UseRedwood(HostingEnvironment.ApplicationPhysicalPath, "my/virt/directory");

            config.RouteTable.Add("Sample1", "Sample1", "Views/sample1.rwhtml", null);
            config.RouteTable.Add("Sample2", "Sample2", "Views/sample2.rwhtml", null);
        }
    }
}
