using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using DotVVM.Samples.SeleniumGenerator.ANC.Services;
using DotVVM.Samples.SeleniumGenerator.ANC.DAL;

namespace DotVVM.Samples.SeleniumGenerator.ANC
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json");
            
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
            services.AddAuthorization();
            services.AddWebEncoders();
            services.AddTransient(typeof(StudentService));
            services.AddTransient(typeof(UserService));
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<StudentDbContext>(options =>
                {
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                });
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<StudentDbContext>()
                .AddDefaultTokenProviders();
			services.ConfigureApplicationCookie(o => { o.LoginPath = new PathString("/Authentication/SignIn"); });
            services.AddDotVVM<DotvvmStartup>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

			app.UseAuthentication();
            // use DotVVM
            var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(env.ContentRootPath);
            dotvvmConfiguration.AssertConfigurationIsValid();
            
            // use static files
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(env.WebRootPath)
            });
        }
    }
}
