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
                                    OnApplyRedirect = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.OwinContext, c.RedirectUri)
                                }
                            })
                    ),
                    new SwitchMiddlewareCase(
                        c => c.Request.Uri.PathAndQuery.StartsWith("/ComplexSamples/SPARedirect"), next =>
                            new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions {
                                LoginPath = new PathString("/ComplexSamples/SPARedirect/login"),
                                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                                Provider = new CookieAuthenticationProvider {
                                    OnApplyRedirect = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.OwinContext, c.RedirectUri)
                                }
                            })
                    ),
                    new SwitchMiddlewareCase(
                        c => c.Request.Uri.PathAndQuery.StartsWith("/ControlSamples/AuthenticatedView")
                            || c.Request.Uri.PathAndQuery.StartsWith("/ControlSamples/RoleView")
                            || c.Request.Uri.PathAndQuery.StartsWith("/ControlSamples/ClaimView"), next =>
                            new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions {
                                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
                            })
                    )
                }
            );
            
            app.UseDotVVM<DotvvmStartup>(GetApplicationPath(), builder: b =>
            {
                b.ConfigureTempStorages("Temp");
            });
            
            app.UseStaticFiles();
        }

        private string GetApplicationPath()
            => Path.Combine(Path.GetDirectoryName(HostingEnvironment.ApplicationPhysicalPath.TrimEnd('\\', '/')), "DotVVM.Samples.Common");
    }
}