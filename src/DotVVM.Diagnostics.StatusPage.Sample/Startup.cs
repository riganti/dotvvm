using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Diagnostics.StatusPage.Sample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
            services.AddAuthorization();
            services.AddWebEncoders();
            services.AddDotVVM<DotvvmStartup>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDotVVM<DotvvmStartup>();
            app.UseStaticFiles();
        }
    }
}
