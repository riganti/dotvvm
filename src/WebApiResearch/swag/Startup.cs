using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using swag.DataStore;
using swag.Model;

namespace swag
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddDotVVM(options =>
            {
                options.AddDefaultTempStorages("temp");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            app.UseDotVVM<DotvvmStartup>(env.ContentRootPath);

            SeedDatabase();
        }

        private static void SeedDatabase()
        {
            var generator = new DataGenerator(1);

            var database = new Database();
            database.Companies = generator.GetCollection(7, i => new Company()
                {
                    Id = i,
                    Name = generator.GetWords(3, 30, Casing.FirstUpper)
                })
                .ToList();

            database.Orders = generator.GetCollection(300, i => new Order()
                {
                    Id = i,
                    CompanyId = generator.GetCollectionItem(database.Companies).Id,
                    Date = generator.GetDate(TimeSpan.FromDays(500), TimeSpan.FromDays(0)),
                    Number = generator.GetString(8, Casing.AllUpper),
                    OrderItems = generator.GetCollection(10, c => new OrderItem()
                    {
                        Id = c,
                        Amount = generator.GetDecimal(0, 1000),
                        Discount = generator.GetBoolean() ? generator.GetDecimal(0, 50) : (decimal?) null,
                        IsOnStock = generator.GetBoolean(),
                        Text = generator.GetWords(10, 8, Casing.AllLower)
                    })
                })
                .ToList();

            Database.Instance = database;
        }
    }
}
