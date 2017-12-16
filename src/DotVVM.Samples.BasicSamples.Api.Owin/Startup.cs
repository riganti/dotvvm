using System;
using System.Threading.Tasks;
using System.Web.Http;
using DotVVM.Samples.BasicSamples.Api.Common.DataStore;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(DotVVM.Samples.BasicSamples.Api.Owin.Startup))]

namespace DotVVM.Samples.BasicSamples.Api.Owin
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var database = new Database();
            database.SeedData();
            Database.Instance = database;

            // Web API
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            SwaggerConfig.Register(config);
            app.UseWebApi(config);
        }
    }
}
