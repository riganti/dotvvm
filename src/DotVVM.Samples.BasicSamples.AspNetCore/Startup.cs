using System;
using System.Collections.Generic;
using System.IO;
using DotVVM.Framework;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Storage;
using DotVVM.Framework.Security;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.DataProtection;

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
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/ComplexSamples/Auth/Login"),
                AuthenticationScheme = "Scheme1"
            });
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/ComplexSamples/SPARedirect/login"),
                AuthenticationScheme = "Scheme2"
            });
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Scheme3"
            });

            var applicationPhysicalPath = Path.Combine(Path.GetDirectoryName(env.ContentRootPath), "DotVVM.Samples.Common");

            // use DotVVM
            DotvvmConfiguration dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath);
            //services.AddSingleton<IViewModelProtector, DefaultViewModelProtector>();
            //services.AddSingleton<ICsrfProtector, DefaultCsrfProtector>();
            //services.AddSingleton<IDotvvmViewBuilder, DefaultDotvvmViewBuilder>();
            //services.AddSingleton<IViewModelSerializer, DefaultViewModelSerializer>();
            dotvvmConfiguration.Debug = true;

            // use static files
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(applicationPhysicalPath)
            });
        }
    }
}