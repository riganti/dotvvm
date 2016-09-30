using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotVVM.Samples.BasicSamples
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(o => o.ResourcesPath = "Resources");

            services.AddAuthentication();

            services
                .AddDotVVM()
                .ConfigureUploadedFileStorage("Temp");
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var supportedCultures = new[] {
                new CultureInfo("en-US"),
                new CultureInfo("cs-CZ")
            };

            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                LoginPath = new PathString("/ComplexSamples/Auth/Login"),
                AuthenticationScheme = "Scheme1",
                Events = new CookieAuthenticationEvents {
                    OnRedirectToReturnUrl = c => DotvvmAuthentication.ApplyRedirect(c.HttpContext, c.RedirectUri),
                    OnRedirectToAccessDenied = c => DotvvmAuthentication.SetStatusCode(c.HttpContext, 403),
                    OnRedirectToLogin = c => DotvvmAuthentication.ApplyRedirect(c.HttpContext, c.RedirectUri),
                    OnRedirectToLogout = c => DotvvmAuthentication.ApplyRedirect(c.HttpContext, c.RedirectUri)
                }
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                LoginPath = new PathString("/ComplexSamples/SPARedirect/login"),
                AuthenticationScheme = "Scheme2",
                Events = new CookieAuthenticationEvents {
                    OnRedirectToReturnUrl = c => DotvvmAuthentication.ApplyRedirect(c.HttpContext, c.RedirectUri),
                    OnRedirectToAccessDenied = c => DotvvmAuthentication.SetStatusCode(c.HttpContext, 403),
                    OnRedirectToLogin = c => DotvvmAuthentication.ApplyRedirect(c.HttpContext, c.RedirectUri),
                    OnRedirectToLogout = c => DotvvmAuthentication.ApplyRedirect(c.HttpContext, c.RedirectUri)
                }
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationScheme = "Scheme3"
            });

            app.UseRequestLocalization(new RequestLocalizationOptions {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                RequestCultureProviders = new List<IRequestCultureProvider> {
                    new QueryStringRequestCultureProvider { QueryStringKey = "lang" }
                }
            });

            app.UseDotVVM<DotvvmStartup>(GetApplicationPath(env));
            app.UseStaticFiles();
        }

        private string GetApplicationPath(IHostingEnvironment env)
            => Path.Combine(Path.GetDirectoryName(env.ContentRootPath), "DotVVM.Samples.Common");
    }
}