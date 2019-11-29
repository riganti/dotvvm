using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Core.Common;
using DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters;
using DotVVM.Samples.BasicSamples.Api.AspNetCore.Controllers;
using DotVVM.Samples.BasicSamples.Api.Common.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
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
            var knownTypesOptions = Options.Create(new DotvvmApiOptions());

            knownTypesOptions.Value.AddKnownType(typeof(Company<string>));

            var options = new SwaggerGeneratorOptions
            {
                DocInclusionPredicate = (version, api) => true,
                OperationFilters =
                {
                    new RemoveReadOnlyFromUriParametersOperationFilter(),
                    new RemoveBindNoneFromUriParametersOperationFilter(),
                    new AddAsObjectOperationFilter(knownTypesOptions)
                },
                DocumentFilters =
                {
                    new HandleKnownTypesDocumentFilter(knownTypesOptions)
                },
                SwaggerDocs =
                {
                    { "v1", new Info() { Title = "Test API", Version = "v1" }}
                }
            };

            var serviceCollection = new ServiceCollection()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton<IHostingEnvironment, HostingEnvironment>()
                .AddSingleton<DiagnosticSource>(p => new DiagnosticListener("test"))
                .AddLogging();

            serviceCollection.AddMvc(setup => setup.Conventions.Add(new ApiExplorerVisibilityEnabledConvention()))
                .AddApplicationPart(typeof(CompaniesController).Assembly);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var apiDescriptionGroupCollectionProvider = serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            var schemaSettings = new SchemaRegistryOptions()
            {
                SchemaFilters =
                {
                    new AddTypeToModelSchemaFilter()
                }
            };

            var schemaRegistryFactory = new SchemaRegistryFactory(new JsonSerializerSettings(), schemaSettings);
            var generator = new SwaggerGenerator(apiDescriptionGroupCollectionProvider, schemaRegistryFactory, options);
            document = generator.GetSwagger("v1");
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_NameAndKnownTypeAnnotation_Added()
        {
            var definition = document.Definitions["IPagingOptions"];

            Assert.AreEqual("DotVVM.Framework.Controls.IPagingOptions", definition.Extensions[ApiConstants.DotvvmKnownTypeKey]);
            Assert.AreEqual("PageIndex", definition.Properties["PageIndex"].Extensions[ApiConstants.DotvvmNameKey]);
            Assert.AreEqual("PageSize", definition.Properties["PageSize"].Extensions[ApiConstants.DotvvmNameKey]);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_GenericNameAndKnownTypeAnnotation_Added()
        {
            var definition = document.Definitions["GridViewDataSet[Company[String]]"];

            Assert.AreEqual("DotVVM.Framework.Controls.GridViewDataSet<Company[String]>",
                definition.Extensions[ApiConstants.DotvvmKnownTypeKey]);

            Assert.AreEqual("IsRefreshRequired", definition.Properties["IsRefreshRequired"].Extensions[ApiConstants.DotvvmNameKey]);
            Assert.AreEqual("Items", definition.Properties["Items"].Extensions[ApiConstants.DotvvmNameKey]);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_GenericKnownTypeAnnotation_FullNameParameter()
        {
            var definition = document.Definitions["Company[String]"];

            Assert.AreEqual("DotVVM.Samples.BasicSamples.Api.Common.Model.Company<System.String>",
                definition.Extensions[ApiConstants.DotvvmKnownTypeKey]);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_MixedGenericKnownTypeAnnotation_Exist()
        {
            var definition = document.Definitions["GridViewDataSet[Company[Boolean]]"];

            Assert.AreEqual("DotVVM.Framework.Controls.GridViewDataSet<Company[Boolean]>", definition.Extensions[ApiConstants.DotvvmKnownTypeKey]);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_KnownTypeAnnotation_NotAdded()
        {
            var definition = document.Definitions["Order"];

            Assert.IsFalse(definition.Extensions.ContainsKey(ApiConstants.DotvvmKnownTypeKey));
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_WrapperTypeAnnotation_OneArgument()
        {
            var operation1 = document.Paths["/api/Companies/sorted"].Get;
            var param1 = operation1.Parameters.Single(p => p.Name == "sortingOptions.SortDescending");
            Assert.AreEqual("DotVVM.Framework.Controls.ISortingOptions, DotVVM.Core", param1.Extensions["x-dotvvm-wrapperType"]);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_WrapperTypeAnnotation_TwoArguments()
        {
            var operation = document.Paths["/api/Companies/sortedandpaged"].Get;
            var param1 = operation.Parameters.Single(p => p.Name == "sortingOptions.SortDescending");
            var param2 = operation.Parameters.Single(p => p.Name == "pagingOptions.PageSize");
            Assert.AreEqual("DotVVM.Framework.Controls.ISortingOptions, DotVVM.Core", param1.Extensions["x-dotvvm-wrapperType"]);
            Assert.AreEqual("DotVVM.Framework.Controls.IPagingOptions, DotVVM.Core", param2.Extensions["x-dotvvm-wrapperType"]);
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
