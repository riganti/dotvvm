using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Api.Swashbuckle.AspNetCore;
using DotVVM.Samples.BasicSamples.Api.AspNetCore.DataStore;
using DotVVM.Samples.BasicSamples.Api.AspNetCore.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore
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
            services.AddMvc()
                .AddJsonOptions(opt => {
                    opt.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            services.AddDotVVM(options => {
                options.AddDefaultTempStorages("temp");
            });

            services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new Info() { Title = "DotVVM Test API", Version = "v1" });

                options.EnableDotvvmIntegration();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors(p =>
            {
                p.AllowAnyOrigin();
                p.AllowAnyMethod();
                p.AllowAnyHeader();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Northwind API");
            });

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
                    Name = Faker.Company.Name(),
                    Owner = Faker.Name.FullName(Faker.NameFormats.WithPrefix)
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
                            Discount = generator.GetBoolean() ? generator.GetDecimal(0, 50) : (decimal?)null,
                            IsOnStock = generator.GetBoolean(),
                            Text = generator.GetWords(10, 8, Casing.AllLower)
                        })
                })
                .ToList();

            Database.Instance = database;
        }
    }
}
