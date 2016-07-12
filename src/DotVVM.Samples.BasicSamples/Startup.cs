using System;
using System.Collections.Generic;
using System.IO;
using DotVVM.Framework;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Internal;

namespace DotVVM.Samples.BasicSamples
{
	public class Startup
	{
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			
       //     app.UseMiddleware<SwitchMiddleware>(
       //         new List<SwitchMiddlewareCase>() {
       //             new SwitchMiddlewareCase(
       //                 c => c.Request.Uri.PathAndQuery.StartsWith("/ComplexSamples/Auth"), next =>
       //                 new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions()
       //                 {
       //                     LoginPath = new PathString("/ComplexSamples/Auth/Login"),
							//AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme,
							
       //                     Provider = new CookieAuthenticationProvider()
       //                     {
       //                         OnApplyRedirect = c =>
       //                         {
       //                             // redirect to login page on 401 request
       //                             if (c.Response.StatusCode == 401 && c.Request.Method == "GET")
       //                             {
       //                                 c.Response.StatusCode = 302;
       //                                 c.Response.Headers["Location"] = c.RedirectUri;
       //                             }
       //                             // do not do anything on redirection to returnurl
       //                             // to not return page when ViewModel is expected
       //                             // we should implement this in DotVVM framework,
       //                             // not samples
       //                         }
       //                     }
       //                 })
       //             ),
       //             new SwitchMiddlewareCase(
       //                 c => c.Request.Uri.PathAndQuery.StartsWith("/ComplexSamples/SPARedirect"), next =>
       //                 new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions()
       //                 {
       //                     LoginPath = new PathString("/ComplexSamples/SPARedirect/login"),
       //                     AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
       //                     Provider = new CookieAuthenticationProvider()
       //                     {
       //                         OnApplyRedirect = c =>
       //                         {
       //                             // redirect to login page on 401 request
       //                             if (c.Response.StatusCode == 401 && c.Request.Method == "GET")
       //                             {
       //                                 c.Response.StatusCode = 302;
       //                                 c.Response.Headers["Location"] = c.RedirectUri;
       //                             }
       //                             // do not do anything on redirection to returnurl
       //                             // to not return page when ViewModel is expected
       //                             // we should implement this in DotVVM framework,
       //                             // not samples
       //                         }
       //                     }
       //                 })
       //             ),
       //              new SwitchMiddlewareCase(
       //                 c => c.Request.Uri.PathAndQuery.StartsWith("/ControlSamples/AuthenticatedView")
       //                     || c.Request.Uri.PathAndQuery.StartsWith("/ControlSamples/RoleView"), next =>
       //                 new CookieAuthenticationMiddleware(next, app, new CookieAuthenticationOptions()
       //                 {
       //                     AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie                            
       //                 })
       //             ),
       //         }
       //     );

            var applicationPhysicalPath = env.WebRootPath;

            // use DotVVM
            DotvvmConfiguration dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath);
            dotvvmConfiguration.Debug = true;

            dotvvmConfiguration.ServiceLocator.RegisterSingleton<IUploadedFileStorage>(
                () => new FileSystemUploadedFileStorage(Path.Combine(applicationPhysicalPath, "Temp"), TimeSpan.FromMinutes(30)));

            // use static files
            app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(applicationPhysicalPath)
			});
        }
    }
}