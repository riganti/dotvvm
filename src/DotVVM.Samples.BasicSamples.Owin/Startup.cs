using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Storage;
using DotVVM.Samples.BasicSamples;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace DotVVM.Samples.BasicSamples
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use<SwitchMiddleware>(
                new List<SwitchMiddlewareCase> {
                    new SwitchMiddlewareCase(
                        c => c.Request.Uri.PathAndQuery.StartsWith("/ComplexSamples/Auth"), next =>
                            new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions {
                                LoginPath = new PathString("/ComplexSamples/Auth/Login"),
                                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                                Provider = new CookieAuthenticationProvider {
                                    OnApplyRedirect = c => DotvvmAuthentication.ApplyRedirect(c.OwinContext, c.RedirectUri)
                                }
                            })
                    ),
                    new SwitchMiddlewareCase(
                        c => c.Request.Uri.PathAndQuery.StartsWith("/ComplexSamples/SPARedirect"), next =>
                            new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions {
                                LoginPath = new PathString("/ComplexSamples/SPARedirect/login"),
                                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                                Provider = new CookieAuthenticationProvider {
                                    OnApplyRedirect = c => DotvvmAuthentication.ApplyRedirect(c.OwinContext, c.RedirectUri)
                                }
                            })
                    ),
                    new SwitchMiddlewareCase(
                        c => c.Request.Uri.PathAndQuery.StartsWith("/ControlSamples/AuthenticatedView")
                            || c.Request.Uri.PathAndQuery.StartsWith("/ControlSamples/RoleView"), next =>
                            new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions {
                                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
                            })
                    )
                }
            );

            var applicationPhysicalPath = Path.Combine(Path.GetDirectoryName(HostingEnvironment.ApplicationPhysicalPath.TrimEnd('\\', '/')), "DotVVM.Samples.Common");

            // use DotVVM
            var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath, registerServices: collection =>
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