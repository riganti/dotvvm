using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters;
using DotVVM.Samples.BasicSamples.Api.AspNetCore.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Tests
{
    [TestClass]
    public class SwaggerFileGenerationTests
    {

        private static SwaggerDocument document;

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            var options = new SwaggerGeneratorSettings();
            options.DocInclusionPredicate = (version, api) => true;
            options.OperationFilters.Add(new RemoveReadOnlyFromUriParametersOperationFilter());
            options.OperationFilters.Add(new RemoveBindNoneFromUriParametersOperationFilter());
            options.OperationFilters.Add(new AddAsObjectAnnotationOperationFilter());
            options.OperationFilters.Add(new HandleGridViewDataSetReturnType());
            options.SwaggerDocs.Add("v1", new Info() { Title = "Test API", Version = "v1" });

            var serviceCollection = new ServiceCollection()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton<IHostingEnvironment, HostingEnvironment>()
                .AddLogging();

            serviceCollection.AddMvc(setup => {
                    setup.Conventions.Add(new ApiExplorerVisibilityEnabledConvention());
                })
                .AddApplicationPart(typeof(CompaniesController).Assembly);
                
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            var apiDescriptionGroupCollectionProvider = serviceProvider.GetService<IApiDescriptionGroupCollectionProvider>();
            
            var schemaRegistryFactory = new SchemaRegistryFactory(new JsonSerializerSettings(), new SchemaRegistrySettings());
            var generator = new SwaggerGenerator(apiDescriptionGroupCollectionProvider, schemaRegistryFactory, options);
            document = generator.GetSwagger("v1");
        }


        [TestMethod]
        public void Swashbuckle_AspNetCore_ReturnsDataSetAnnotation_NotAdded()
        {
            var operation = document.Paths["/api/Companies"].Get;
            Assert.IsFalse(operation.Extensions.ContainsKey("x-dotvvm-returnsDataSet"));
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_ReturnsDataSetAnnotation_Added()
        {
            var operation = document.Paths["/api/Companies/sorted"].Get;
            Assert.AreEqual("true", operation.Extensions["x-dotvvm-returnsDataSet"].ToString());
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_WrapperTypeAnnotation_OneArgument()
        {
            var operation1 = document.Paths["/api/Companies/sorted"].Get;
            var param1 = operation1.Parameters.Single(p => p.Name == "sortingOptions.SortDescending");
            Assert.AreEqual("DotVVM.Framework.Controls.SortingOptions, DotVVM.Core", param1.Extensions["x-dotvvm-wrapperType"]);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_WrapperTypeAnnotation_TwoArguments()
        {
            var operation = document.Paths["/api/Companies/sortedandpaged"].Get;
            var param1 = operation.Parameters.Single(p => p.Name == "sortingOptions.SortDescending");
            var param2 = operation.Parameters.Single(p => p.Name == "pagingOptions.PageSize");
            Assert.AreEqual("DotVVM.Framework.Controls.SortingOptions, DotVVM.Core", param1.Extensions["x-dotvvm-wrapperType"]);
            Assert.AreEqual("DotVVM.Framework.Controls.PagingOptions, DotVVM.Core", param2.Extensions["x-dotvvm-wrapperType"]);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_WrapperTypeAnnotation_NotAdded()
        {
            var operation = document.Paths["/api/Orders/{orderId}"].Get;
            var param = operation.Parameters.Single(p => p.Name == "orderId");
            Assert.IsFalse(param.Extensions.ContainsKey("x-dotvvm-wrapperType"));
        }
    }
    
    public class ApiExplorerVisibilityEnabledConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (controller.ApiExplorer.IsVisible == null)
                {
                    controller.ApiExplorer.IsVisible = true;
                    controller.ApiExplorer.GroupName = controller.ControllerName;
                }
            }
        }
    }
}
