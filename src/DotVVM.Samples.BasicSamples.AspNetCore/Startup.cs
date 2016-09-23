using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using DotVVM.Framework.Storage;
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
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
            services.AddWebEncoders();
            services.AddDotvvmServices();
            services.AddSingleton<IUploadedFileStorage>(s => new FileSystemUploadedFileStorage(Path.Combine(s.GetService<DotvvmConfiguration>().ApplicationPhysicalPath, "Temp"), TimeSpan.FromMinutes(30)));
            services.AddSingleton<IViewModelProtector, DefaultViewModelProtector>();
            services.AddSingleton<ICsrfProtector, DefaultCsrfProtector>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            var applicationPhysicalPath = Path.Combine(Path.GetDirectoryName(env.ContentRootPath), "DotVVM.Samples.Common");

            // use DotVVM
            var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath);
            //services.AddSingleton<IViewModelProtector, DefaultViewModelProtector>();
            //services.AddSingleton<ICsrfProtector, DefaultCsrfProtector>();
            //services.AddSingleton<IDotvvmViewBuilder, DefaultDotvvmViewBuilder>();
            //services.AddSingleton<IViewModelSerializer, DefaultViewModelSerializer>();
            dotvvmConfiguration.Debug = true;

            // use static files
            app.UseStaticFiles();
        }
    }
}