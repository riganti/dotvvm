using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using DotVVM.Core.Common;
using DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters;
using DotVVM.Samples.BasicSamples.Api.AspNetCore.Controllers;
using DotVVM.Samples.BasicSamples.Api.Common.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Tests
{
    [TestClass]
    public class SwaggerFileGenerationTests
    {
        private static OpenApiDocument document;

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
                    { "v1", new OpenApiInfo() { Title = "Test API", Version = "v1" }}
                }
            };

            var serviceCollection = new ServiceCollection()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton<DiagnosticSource>(p => new DiagnosticListener("test"))
                .AddLogging();

            serviceCollection.AddMvc(setup => setup.Conventions.Add(new ApiExplorerVisibilityEnabledConvention()))
                .AddApplicationPart(typeof(CompaniesController).Assembly);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var apiDescriptionGroupCollectionProvider = serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            string GetCustomSchemaId(Type modelType)
            {
                if (!modelType.IsConstructedGenericType) return modelType.Name.Replace("[]", "Array");

                var generics = modelType.GetGenericArguments()
                    .Select(genericArg => GetCustomSchemaId(genericArg))
                    .Aggregate((previous, current) => previous + current);

                return $"{modelType.Name.Split('`').First()}[{generics}]";
            }

            var schemaGeneratorOptions = new SchemaGeneratorOptions() {
                SchemaFilters =
                {
                    new AddTypeToModelSchemaFilter()
                },
                SchemaIdSelector = type => GetCustomSchemaId(type)
            };

            var schemaGenerator = new SchemaGenerator(schemaGeneratorOptions, new JsonSerializerDataContractResolver(new JsonSerializerOptions()));
            var swaggerGenerator = new SwaggerGenerator(options, apiDescriptionGroupCollectionProvider, schemaGenerator);

            document = swaggerGenerator.GetSwagger("v1");
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_NameAndKnownTypeAnnotation_Added()
        {
            var definition = document.Components.Schemas["IPagingOptions"];

            var knownTypeKeyWrapper = definition.Extensions[ApiConstants.DotvvmKnownTypeKey] as OpenApiString;
            Assert.AreEqual("DotVVM.Framework.Controls.IPagingOptions", knownTypeKeyWrapper.Value);

            var nameKeyWrapper = definition.Properties["PageIndex"].Extensions[ApiConstants.DotvvmNameKey] as OpenApiString;
            Assert.AreEqual("PageIndex", nameKeyWrapper.Value);

            nameKeyWrapper = definition.Properties["PageSize"].Extensions[ApiConstants.DotvvmNameKey] as OpenApiString;
            Assert.AreEqual("PageSize", nameKeyWrapper.Value);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_GenericNameAndKnownTypeAnnotation_Added()
        {
            var definition = document.Components.Schemas["GridViewDataSet[Company[String]]"];

            var knownTypeKeyWrapper = definition.Extensions[ApiConstants.DotvvmKnownTypeKey] as OpenApiString;
            Assert.AreEqual("DotVVM.Framework.Controls.GridViewDataSet<Company[String]>", knownTypeKeyWrapper.Value);

            var nameKeyWrapper = definition.Properties["IsRefreshRequired"].Extensions[ApiConstants.DotvvmNameKey] as OpenApiString;
            Assert.AreEqual("IsRefreshRequired", nameKeyWrapper.Value);

            nameKeyWrapper = definition.Properties["Items"].Extensions[ApiConstants.DotvvmNameKey] as OpenApiString;
            Assert.AreEqual("Items", nameKeyWrapper.Value);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_GenericKnownTypeAnnotation_FullNameParameter()
        {
            var definition = document.Components.Schemas["Company[String]"];

            var wrappedKnownTypeKey = definition.Extensions[ApiConstants.DotvvmKnownTypeKey] as OpenApiString;
            Assert.AreEqual("DotVVM.Samples.BasicSamples.Api.Common.Model.Company<System.String>", wrappedKnownTypeKey.Value);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_MixedGenericKnownTypeAnnotation_Exist()
        {
            var definition = document.Components.Schemas["GridViewDataSet[Company[Boolean]]"];

            var wrappedKnownTypeKey = definition.Extensions[ApiConstants.DotvvmKnownTypeKey] as OpenApiString;
            Assert.AreEqual("DotVVM.Framework.Controls.GridViewDataSet<Company[Boolean]>", wrappedKnownTypeKey.Value);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_KnownTypeAnnotation_NotAdded()
        {
            var definition = document.Components.Schemas["Order"];

            Assert.IsFalse(definition.Extensions.ContainsKey(ApiConstants.DotvvmKnownTypeKey));
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_WrapperTypeAnnotation_OneArgument()
        {
            var operation1 = document.Paths["/api/Companies/sorted"].Operations[OperationType.Get];
            var param1 = operation1.Parameters.Single(p => p.Name == "sortingOptions.SortDescending");
            var param1WrapperType = param1.Extensions["x-dotvvm-wrapperType"] as OpenApiPrimitive<string>;
            Assert.AreEqual("DotVVM.Framework.Controls.ISortingOptions, DotVVM.Core", param1WrapperType.Value);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_WrapperTypeAnnotation_TwoArguments()
        {
            var operation = document.Paths["/api/Companies/sortedandpaged"].Operations[OperationType.Get];
            var param1 = operation.Parameters.Single(p => p.Name == "sortingOptions.SortDescending");
            var param1WrapperType = param1.Extensions["x-dotvvm-wrapperType"] as OpenApiPrimitive<string>;
            var param2 = operation.Parameters.Single(p => p.Name == "pagingOptions.PageSize");
            var param2WrapperType = param2.Extensions["x-dotvvm-wrapperType"] as OpenApiPrimitive<string>;
            Assert.AreEqual("DotVVM.Framework.Controls.ISortingOptions, DotVVM.Core", param1WrapperType.Value);
            Assert.AreEqual("DotVVM.Framework.Controls.IPagingOptions, DotVVM.Core", param2WrapperType.Value);
        }

        [TestMethod]
        public void Swashbuckle_AspNetCore_WrapperTypeAnnotation_NotAdded()
        {
            var operation = document.Paths["/api/Orders/{orderId}"].Operations[OperationType.Get];
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
