using System.Web.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Microsoft.Extensions.DependencyInjection;

[assembly: OwinStartup(typeof(DotVVM.CompilationStatusPage.Startup))]
namespace DotVVM.CompilationStatusPage
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            // use DotVVM
            var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath, debug: IsInDebugMode(), options: options =>
            {
                options.AddDefaultTempStorages("temp");
            });

            // use static files
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = new PhysicalFileSystem(applicationPhysicalPath)
            });
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
