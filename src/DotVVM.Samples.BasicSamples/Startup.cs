using System;
using System.IO;
using DotVVM.Framework;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using System.Web.Hosting;
using DotVVM.Framework.Storage;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;

[assembly: OwinStartup(typeof(DotVVM.Samples.BasicSamples.Startup))]

namespace DotVVM.Samples.BasicSamples
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                LoginPath = new PathString("/AuthSample/Login"),
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                Provider = new CookieAuthenticationProvider()
                {
                    OnApplyRedirect = c =>
                    {
                        // redirect to login page on 401 request
                        if (c.Response.StatusCode == 401 && c.Request.Method == "GET")
                        {
                            c.Response.StatusCode = 302;
                            c.Response.Headers["Location"] = c.RedirectUri;
                        }
                        // do not do anything on redirection to returnurl
                        // to not return page when ViewModel is expected
                        // we should implement this in DotVVM framework,
                        // not samples
                    }
                }
            });
            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            // use DotVVM
            DotvvmConfiguration dotvvmConfiguration = app.UseDotVVM(applicationPhysicalPath);
            dotvvmConfiguration.RouteTable.RegisterRoutingStrategy(new ViewsFolderBasedRouteStrategy(dotvvmConfiguration));

            dotvvmConfiguration.RouteTable.Add("RepeaterRouteLink-PageDetail", "ControlSamples/Repeater/RouteLink/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml");

            dotvvmConfiguration.ServiceLocator.RegisterSingleton<IUploadedFileStorage>(
                () => new FileSystemUploadedFileStorage(Path.Combine(applicationPhysicalPath, "Temp"), TimeSpan.FromMinutes(30)));

            // use static files
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = new PhysicalFileSystem(applicationPhysicalPath)
            });
        }
    }
}