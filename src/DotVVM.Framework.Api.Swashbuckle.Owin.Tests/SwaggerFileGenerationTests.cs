using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using DotVVM.Core.Common;
using DotVVM.Framework.Api.Swashbuckle.Owin.Filters;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.Api.Common.Model;
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

            var apiOptions = new DotvvmApiOptions();
            apiOptions.AddKnownType(typeof(Company<string>));

            var options = new SwaggerGeneratorOptions(operationFilters: new IOperationFilter[] {
                // default filters used by Swashbuckle
                new HandleFromUriParams(),
                new ApplySwaggerOperationAttributes(),
                new ApplySwaggerResponseAttributes(),
                new ApplySwaggerOperationFilterAttributes(),

                // our custom filters
                new AddAsObjectAnnotationOperationFilter(new DefaultPropertySerialization()),
            }, schemaFilters: new[] { new AddTypeToModelSchemaFilter()
            }, documentFilters: new[] { new HandleKnownTypesDocumentFilter(apiOptions, new DefaultPropertySerialization()) });

            var generator = new SwaggerGenerator(apiExplorer, settings, versions, options);
            document = generator.GetSwagger("http://localhost:61453/", "v1");
        }

        [TestMethod]
        public void Swashbuckle_Owin_NameAndKnownTypeAnnotation_Added()
        {
            var definition = document.definitions["IPagingOptions"];

            Assert.AreEqual("DotVVM.Framework.Controls.IPagingOptions", definition.vendorExtensions[ApiConstants.DotvvmKnownTypeKey]);
            Assert.AreEqual("PageIndex", definition.properties["PageIndex"].vendorExtensions[ApiConstants.DotvvmNameKey]);
            Assert.AreEqual("PageSize", definition.properties["PageSize"].vendorExtensions[ApiConstants.DotvvmNameKey]);
        }


        [TestMethod]
        public void Swashbuckle_Owin_GenericNameAndKnownTypeAnnotation_Added()
        {
            var definition = document.definitions["GridViewDataSet[Company[String]]"];

            Assert.AreEqual("DotVVM.Framework.Controls.GridViewDataSet<Company[String]>",
                definition.vendorExtensions[ApiConstants.DotvvmKnownTypeKey]);

            Assert.AreEqual("IsRefreshRequired", definition.properties["IsRefreshRequired"].vendorExtensions[ApiConstants.DotvvmNameKey]);
            Assert.AreEqual("Items", definition.properties["Items"].vendorExtensions[ApiConstants.DotvvmNameKey]);
        }

        [TestMethod]
        public void Swashbuckle_Owin_GenericKnownTypeAnnotation_FullNameParameter()
        {
            var definition = document.definitions["Company[String]"];

            Assert.AreEqual("DotVVM.Samples.BasicSamples.Api.Common.Model.Company<System.String>",
                definition.vendorExtensions[ApiConstants.DotvvmKnownTypeKey]);
        }

        [TestMethod]
        public void Swashbuckle_Owin_MixedGenericKnownTypeAnnotation_Exist()
        {
            var definition = document.definitions["GridViewDataSet[Company[Boolean]]"];

            Assert.AreEqual("DotVVM.Framework.Controls.GridViewDataSet<Company[Boolean]>", definition.vendorExtensions[ApiConstants.DotvvmKnownTypeKey]);
        }

        [TestMethod]
        public void Swashbuckle_Owin_KnownTypeAnnotation_NotAdded()
        {
            var definition = document.definitions["Order"];

            Assert.IsFalse(definition.vendorExtensions.ContainsKey(ApiConstants.DotvvmKnownTypeKey));
        }

        [TestMethod]
        public void Swashbuckle_Owin_WrapperTypeAnnotation_OneArgument()
        {
            var operation1 = document.paths["/api/companies/sorted"].get;
            var param1 = operation1.parameters.Single(p => p.name == "sortingOptions.SortDescending");
            Assert.AreEqual("DotVVM.Framework.Controls.ISortingOptions, DotVVM.Core", param1.vendorExtensions[ApiConstants.DotvvmWrapperTypeKey]);
        }

        [TestMethod]
        public void Swashbuckle_Owin_WrapperTypeAnnotation_TwoArguments()
        {
            var operation = document.paths["/api/companies/sortedandpaged"].get;
            var param1 = operation.parameters.Single(p => p.name == "sortingOptions.SortDescending");
            var param2 = operation.parameters.Single(p => p.name == "pagingOptions.PageSize");
            Assert.AreEqual("DotVVM.Framework.Controls.ISortingOptions, DotVVM.Core", param1.vendorExtensions[ApiConstants.DotvvmWrapperTypeKey]);
            Assert.AreEqual("DotVVM.Framework.Controls.IPagingOptions, DotVVM.Core", param2.vendorExtensions[ApiConstants.DotvvmWrapperTypeKey]);
        }

        [TestMethod]
        public void Swashbuckle_Owin_WrapperTypeAnnotation_NotAdded()
        {
            var operation = document.paths["/api/orders/{orderId}"].get;
            var param = operation.parameters.Single(p => p.name == "orderId");
            Assert.IsFalse(param.vendorExtensions.ContainsKey(ApiConstants.DotvvmWrapperTypeKey));
        }
    }
}
