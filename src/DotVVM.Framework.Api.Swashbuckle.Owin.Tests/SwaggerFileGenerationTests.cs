using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using DotVVM.Framework.Api.Swashbuckle.Owin.Filters;
using DotVVM.Samples.BasicSamples.Api.Owin.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using Swashbuckle.Swagger.Annotations;
using Swashbuckle.Swagger.FromUriParams;

namespace DotVVM.Framework.Api.Swashbuckle.Owin.Tests
{
    [TestClass]
    public class SwaggerFileGenerationTests
    {
        private static SwaggerDocument document;

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            var t = typeof(CompaniesController);

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.EnsureInitialized();

            var apiExplorer = new ApiExplorer(config);
            var settings = new JsonSerializerSettings();
            var versions = new Dictionary<string, Info> {
                { "v1", new Info() { version = "v1", title = "Test API" } }
            };
            var options = new SwaggerGeneratorOptions(operationFilters: new IOperationFilter[]
            {
                // default filters used by Swashbuckle
                new HandleFromUriParams(),
                new ApplySwaggerOperationAttributes(),
                new ApplySwaggerResponseAttributes(),
                new ApplySwaggerOperationFilterAttributes(),

                // our custom filters
                new AddAsObjectAnnotationOperationFilter(),
                new HandleGridViewDataSetReturnType()
            });
            var generator = new SwaggerGenerator(apiExplorer, settings, versions, options);
            document = generator.GetSwagger("http://localhost:61453/", "v1");
        }

        [TestMethod]
        public void Swashbuckle_Owin_ReturnsDataSetAnnotation_NotAdded()
        {
            var operation = document.paths["/api/companies"].get;
            Assert.IsFalse(operation.vendorExtensions.ContainsKey("x-dotvvm-returnsDataSet"));
        }

        [TestMethod]
        public void Swashbuckle_Owin_ReturnsDataSetAnnotation_Added()
        {
            var operation = document.paths["/api/companies/sorted"].get;
            Assert.AreEqual("true", operation.vendorExtensions["x-dotvvm-returnsDataSet"].ToString());
        }

        [TestMethod]
        public void Swashbuckle_Owin_WrapperTypeAnnotation_OneArgument()
        {
            var operation1 = document.paths["/api/companies/sorted"].get;
            var param1 = operation1.parameters.Single(p => p.name == "sortingOptions.SortDescending");
            Assert.AreEqual("DotVVM.Framework.Controls.ISortingOptions, DotVVM.Core", param1.vendorExtensions["x-dotvvm-wrapperType"]);
        }

        [TestMethod]
        public void Swashbuckle_Owin_WrapperTypeAnnotation_TwoArguments()
        {
            var operation = document.paths["/api/companies/sortedandpaged"].get;
            var param1 = operation.parameters.Single(p => p.name == "sortingOptions.SortDescending");
            var param2 = operation.parameters.Single(p => p.name == "pagingOptions.PageSize");
            Assert.AreEqual("DotVVM.Framework.Controls.ISortingOptions, DotVVM.Core", param1.vendorExtensions["x-dotvvm-wrapperType"]);
            Assert.AreEqual("DotVVM.Framework.Controls.IPagingOptions, DotVVM.Core", param2.vendorExtensions["x-dotvvm-wrapperType"]);
        }

        [TestMethod]
        public void Swashbuckle_Owin_WrapperTypeAnnotation_NotAdded()
        {
            var operation = document.paths["/api/orders/{orderId}"].get;
            var param = operation.parameters.Single(p => p.name == "orderId");
            Assert.IsFalse(param.vendorExtensions.ContainsKey("x-dotvvm-wrapperType"));
        }
    }
}
