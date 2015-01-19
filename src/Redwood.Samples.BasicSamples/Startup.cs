using System.Web.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Redwood.Framework;
using Redwood.Framework.Configuration;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;

[assembly: OwinStartup(typeof(Redwood.Samples.BasicSamples.Startup))]
namespace Redwood.Samples.BasicSamples
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseRedwoodErrorPages();
            app.UseErrorPage();

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                LoginPath = new PathString("/AuthSample/Login"),
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                Provider = new CookieAuthenticationProvider()
                {
                    OnApplyRedirect = c =>
                    {
                        // redirect to login page on 401 request
                        if(c.Response.StatusCode == 401 && c.Request.Method == "GET")
                        {
                            c.Response.StatusCode = 302;
                            c.Response.Headers["Location"] = c.RedirectUri;
                        }
                        // do not do anything on redirection to returnurl
                        // to not return page when ViewModel is expected
                        // we should implement this in Redwood framework,
                        // not samples
                    }
                }
            });

            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            // use Redwood
            RedwoodConfiguration redwoodConfiguration = app.UseRedwood(applicationPhysicalPath);
            redwoodConfiguration.RouteTable.Add("Sample1", "Sample1", "sample1.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample2", "Sample2", "sample2.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample3", "Sample3", "sample3.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample4", "Sample4", "sample4.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample5", "Sample5", "sample5.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample6", "Sample6", "sample6.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample7", "Sample7", "sample7.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample8", "Sample8", "sample8.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample9", "Sample9", "sample9.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample10", "Sample10", "sample10.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample11", "Sample11", "sample11.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("Sample12", "Sample12", "sample12.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("AuthSampleLogin", "AuthSample/Login", "AuthSample/login.rwhtml", null);
            redwoodConfiguration.RouteTable.Add("AuthSamplePage", "AuthSample/SecuredPage", "AuthSample/securedPage.rwhtml", null);

            // use static files
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = new PhysicalFileSystem(applicationPhysicalPath)
            });
        }
    }
}
