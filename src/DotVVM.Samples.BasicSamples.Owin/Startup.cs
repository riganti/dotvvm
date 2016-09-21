using System;
using System.Collections.Generic;
using System.IO;
using DotVVM.Framework;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using System.Web.Hosting;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Storage;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.DependencyInjection;

[assembly: OwinStartup(typeof(DotVVM.Samples.BasicSamples.Startup))]

namespace DotVVM.Samples.BasicSamples
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use<SwitchMiddleware>(
                new List<SwitchMiddlewareCase>() {
                    new SwitchMiddlewareCase(
                        c => c.Request.Uri.PathAndQuery.StartsWith("/ComplexSamples/Auth"), next =>
                        new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions()
                        {
                            LoginPath = new PathString("/ComplexSamples/Auth/Login"),
                            AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                            //Provider = new CookieAuthenticationProvider()
                            //{
                            //    OnApplyRedirect = c =>
                            //    {
                            //        // redirect to login page on 401 request
                            //        if (c.Response.StatusCode == 401 && c.Request.Method == "GET")
                            //        {
                            //            c.Response.StatusCode = 302;
                            //            c.Response.Headers["Location"] = c.RedirectUri;
                            //        }
                            //        // do not do anything on redirection to returnurl
                            //        // to not return page when ViewModel is expected
                            //        // we should implement this in DotVVM framework,
                            //        // not samples
                            //    }
                            //}
                        })
                    ),
                    new SwitchMiddlewareCase(
                        c => c.Request.Uri.PathAndQuery.StartsWith("/ComplexSamples/SPARedirect"), next =>
                        new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions()
                        {
                            LoginPath = new PathString("/ComplexSamples/SPARedirect/login"),
                            AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                            //Provider = new CookieAuthenticationProvider()
                            //{
                            //    OnApplyRedirect = c =>
                            //    {
                            //        // redirect to login page on 401 request
                            //        if (c.Response.StatusCode == 401 && c.Request.Method == "GET")
                            //        {
                            //            c.Response.StatusCode = 302;
                            //            c.Response.Headers["Location"] = c.RedirectUri;
                            //        }
                            //        // do not do anything on redirection to returnurl
                            //        // to not return page when ViewModel is expected
                            //        // we should implement this in DotVVM framework,
                            //        // not samples
                            //    }
                            //}
                        })
                    ),
                     new SwitchMiddlewareCase(
                        c => c.Request.Uri.PathAndQuery.StartsWith("/ControlSamples/AuthenticatedView")
                            || c.Request.Uri.PathAndQuery.StartsWith("/ControlSamples/RoleView"), next =>
                        new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions()
                        {
                            AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
                        })
                    ),
                }
            );

            var applicationPhysicalPath = Path.Combine(Path.GetDirectoryName(HostingEnvironment.ApplicationPhysicalPath.TrimEnd('\\', '/')), "DotVVM.Samples.Common");

            // use DotVVM
            DotvvmConfiguration dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath, registerServices: collection =>
            {
                collection.AddSingleton<IUploadedFileStorage>(
                    p => new FileSystemUploadedFileStorage(Path.Combine(applicationPhysicalPath, "Temp"), TimeSpan.FromMinutes(30)));
            });
            dotvvmConfiguration.Debug = true;

            // use static files
            app.UseStaticFiles();
        }
    }
}
