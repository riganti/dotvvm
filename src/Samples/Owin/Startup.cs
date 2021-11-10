using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using DotVVM.Framework.Hosting;
using DotVVM.Samples.BasicSamples;
using DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.DependencyInjection;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.StaticCommand;
using DotVVM.Samples.Common;
using System;
using System.Configuration;
using DotVVM.Framework.Utils;
using System.Linq;

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
                                AuthenticationType = "ApplicationCookie",
                                Provider = new CookieAuthenticationProvider {
                                    OnApplyRedirect = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.OwinContext, c.RedirectUri)
                                }
                            })
                    ),
                    new SwitchMiddlewareCase(
                        c => c.Request.Uri.PathAndQuery.StartsWith("/ComplexSamples/SPARedirect"), next =>
                            new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions {
                                LoginPath = new PathString("/ComplexSamples/SPARedirect/login"),
                                AuthenticationType = "ApplicationCookie",
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
                                AuthenticationType = "ApplicationCookie"
                            })
                    )
                }
            );

            var config = app.UseDotVVM<DotvvmStartup, DotvvmServiceConfigurator>(GetApplicationPath(), modifyConfiguration: c  => {
                c.RouteTable.Add("AuthorizedPresenter", "ComplexSamples/Auth/AuthorizedPresenter", provider => new AuthorizedPresenter());

                if (c.ExperimentalFeatures.ExplicitAssemblyLoading.Enabled)
                {
                    c.Markup.AddAssembly(typeof(Startup).Assembly.FullName);
                }
            });

            app.UseDotvvmHotReload();

#if AssertConfiguration
            // this compilation symbol is set by CI server
            config.AssertConfigurationIsValid();
#endif
            app.UseStaticFiles();
        }

        private string GetApplicationPath()
            => Path.Combine(Path.GetDirectoryName(HostingEnvironment.ApplicationPhysicalPath.TrimEnd('\\', '/')), "Common");
    }
}
