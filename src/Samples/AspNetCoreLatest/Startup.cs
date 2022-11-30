using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.Auth;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.StaticCommand;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace DotVVM.Samples.BasicSamples
{
    public class Startup
    {
        public class BindingTestResolvers
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("Scheme1")
                .AddCookie("Scheme1", o => {
                    o.LoginPath = new PathString("/ComplexSamples/Auth/Login");
                    o.Events = new CookieAuthenticationEvents {
                        OnRedirectToReturnUrl = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.HttpContext, c.RedirectUri),
                        OnRedirectToAccessDenied = c => DotvvmAuthenticationHelper.ApplyStatusCodeResponse(c.HttpContext, 403),
                        OnRedirectToLogin = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.HttpContext, c.RedirectUri),
                        OnRedirectToLogout = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.HttpContext, c.RedirectUri)
                    };
                });

            services.AddAuthentication("Scheme2")
                .AddCookie("Scheme2", o => {
                    o.LoginPath = new PathString("/ComplexSamples/SPARedirect/login");
                    o.Events = new CookieAuthenticationEvents {
                        OnRedirectToReturnUrl = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.HttpContext, c.RedirectUri),
                        OnRedirectToAccessDenied = c => DotvvmAuthenticationHelper.ApplyStatusCodeResponse(c.HttpContext, 403),
                        OnRedirectToLogin = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.HttpContext, c.RedirectUri),
                        OnRedirectToLogout = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.HttpContext, c.RedirectUri)
                    };
                });

            services.AddAuthentication("Scheme3")
                .AddCookie("Scheme3");

            services.AddHealthChecks();

            services.AddLocalization(o => o.ResourcesPath = "Resources");

            services.AddDotVVM<DotvvmServiceConfigurator>();

            services.Configure<BindingCompilationOptions>(o => {
                o.TransformerClasses.Add(new BindingTestResolvers());
            });

            services.AddSingleton<IGreetingComputationService, HelloGreetingComputationService>();

            services.AddScoped<ViewModelScopedDependency>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseRouting();
            app.UseAuthentication();

            var config = app.UseDotVVM<DotvvmStartup>(GetApplicationPath(env), modifyConfiguration: c => {
                c.RouteTable.Add("AuthorizedPresenter", "ComplexSamples/Auth/AuthorizedPresenter", provider => new AuthorizedPresenter());

                if (c.ExperimentalFeatures.ExplicitAssemblyLoading.Enabled)
                {
                    c.Markup.AddAssembly(typeof(Startup).Assembly.FullName);
                    c.Markup.AddAssembly("System.Security.Claims");
                }
            });

#if AssertConfiguration
            // this compilation symbol is set by CI server
            config.AssertConfigurationIsValid();
#endif
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapHealthChecks("/health");
            });

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings.Add(".dll", "application/octet-stream");
            contentTypeProvider.Mappings.Add(".symbols", "application/octet-stream");
            contentTypeProvider.Mappings.Add(".blat", "application/octet-stream");
            contentTypeProvider.Mappings.Add(".dat", "application/octet-stream");
            app.UseStaticFiles(new StaticFileOptions() {
                RequestPath = new PathString("/dotvvmResource/dotvvm--interop--dotnet-wasm"),
                FileProvider = new PhysicalFileProvider("D:\\Work\\Dotvvm\\src\\Samples\\CSharpClient\\bin\\Debug\\net7.0\\browser-wasm\\AppBundle"),
                ContentTypeProvider = contentTypeProvider
            });
            app.UseStaticFiles();

            app.UseEndpoints(endpoints => {
                endpoints.MapDotvvmHotReload();
            });
        }

        private string GetApplicationPath(IWebHostEnvironment env)
            => Path.Combine(Path.GetDirectoryName(env.ContentRootPath), "Common");
    }
}
