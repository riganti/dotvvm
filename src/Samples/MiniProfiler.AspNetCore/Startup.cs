using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling;
using DotVVM.Samples.MiniProfiler.AspNetCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DotVVM.Samples.MiniProfiler.AspNetCore
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
            services.AddAuthorization();
            services.AddWebEncoders();

            services.AddDotVVM<DotvvmStartup>();

            services.AddDbContext<SampleContext>(c => c.UseSqlite("Data Source=sample.db;"));
            services.AddMemoryCache();

            services.AddMiniProfiler(options =>
            {
                options.ResultsAuthorizeAsync = async (e) =>
                {
                    await Task.Delay(100).ConfigureAwait(true);
                    await Task.Delay(50).ConfigureAwait(false);
                    await Task.Yield();
                    return true;
                };
                options.RouteBasePath = "/profiler";
            }).AddEntityFramework();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiniProfiler();

            app.UseDotVVM<DotvvmStartup>(env.ContentRootPath);

            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using (var serviceScope = serviceScopeFactory.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<SampleContext>();
                dbContext.Database.EnsureCreated();
            }
        }
    }
}
