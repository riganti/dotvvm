using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using DotVVM.Samples.BasicSamples.Api.Common.DataStore;
using Microsoft.Owin;
using Newtonsoft.Json;
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
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            SwaggerConfig.Register(config);
            app.UseWebApi(config);
        }
    }
}
