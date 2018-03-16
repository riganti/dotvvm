using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Profiling;

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

            services.AddMemoryCache();

            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
            }).AddEntityFramework();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IMemoryCache cache)
        {
            loggerFactory.AddConsole();

            app.UseMiniProfiler();

            app.UseDotVVM<DotvvmStartup>(env.ContentRootPath);
        }
    }
}
