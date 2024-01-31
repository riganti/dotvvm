using System;
using System.Linq;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class ViewCompilationServiceTests
    {
        static readonly FakeMarkupFileLoader fileLoader;
        static readonly DotvvmConfiguration config;
        static readonly DotvvmViewCompilationService service;
        static ViewCompilationServiceTests()
        {
            fileLoader = new FakeMarkupFileLoader();
            config = DotvvmTestHelper.CreateConfiguration(s => {
                s.AddSingleton<IMarkupFileLoader>(fileLoader);
            });
            config.Markup.AddCodeControls("test", exampleControl: typeof(ControlWithContextInjection));

            config.RouteTable.Add("WithContextInjection", "WithContextInjection", "WithContextInjection.dothtml", null);
            fileLoader.MarkupFiles["WithContextInjection.dothtml"] = """
                @viewModel object
                <test:ControlWithContextInjection />
                """;

            config.RouteTable.Add("WithUnresolvableDependency", "WithUnresolvableDependency", "WithUnresolvableDependency.dothtml", null);
            fileLoader.MarkupFiles["WithUnresolvableDependency.dothtml"] = """
                @viewModel object
                <test:ControlWithUnresolvableDependency />
                """;


            config.RouteTable.Add("WithError", "WithError", "WithError.dothtml", null);
            fileLoader.MarkupFiles["WithError.dothtml"] = """
                @viewModel object
                <test:ThisControlDoesNotExist />
                """;

            config.RouteTable.Add("WithMasterPage", "WithMasterPage", "WithMasterPage.dothtml", null);
            fileLoader.MarkupFiles["WithMasterPage.dothtml"] = """
                @viewModel object
                @masterPage MasterPage.dothtml
                <dot:Content ContentPlaceHolderID=Content>test</dot:Content>
                """;
            fileLoader.MarkupFiles["MasterPage.dothtml"] = """
                @viewModel object
                <head></head>

                <body>
                    <dot:ContentPlaceHolder ID=Content />
                </body>
                """;

            config.RouteTable.Add("NonCompilable", "NonCompilable", null, presenterFactory: _ => throw null);

            config.Freeze();

            service = (DotvvmViewCompilationService)config.ServiceProvider.GetRequiredService<IDotvvmViewCompilationService>();
        }
        [TestMethod]
        public void RequestContextInjection()
        {
            var route = service.GetRoutes().First(r => r.RouteName == "WithContextInjection");
            service.BuildView(route, out _);
            Assert.IsNull(route.Exception);
            Assert.AreEqual(CompilationState.CompletedSuccessfully, route.Status);
        }
        [TestMethod]
        public void InjectionUnresolvableDependency()
        {
            var route = service.GetRoutes().First(r => r.RouteName == "WithUnresolvableDependency");
            service.BuildView(route, out _);
            Assert.AreEqual(CompilationState.CompilationFailed, route.Status);
            Assert.AreEqual("Unable to resolve service for type 'DotVVM.Framework.Tests.Runtime.ControlWithUnresolvableDependency+ThisServiceIsntRegistered' while attempting to activate 'DotVVM.Framework.Tests.Runtime.ControlWithUnresolvableDependency'.", route.Exception);
            Assert.IsNotNull(route.Exception);
        }
        [TestMethod]
        public void ErrorInMarkup()
        {
            var route = service.GetRoutes().First(r => r.RouteName == "WithError");
            service.BuildView(route, out _);
            Assert.AreEqual(CompilationState.CompilationFailed, route.Status);
            Assert.IsNotNull(route.Exception);
            Assert.AreEqual("The control <test:ThisControlDoesNotExist> could not be resolved! Make sure that the tagPrefix is registered in DotvvmConfiguration.Markup.Controls collection!", route.Exception);
        }

        [TestMethod]
        public void MasterPage()
        {
            var route = service.GetRoutes().First(r => r.RouteName == "WithMasterPage");
            service.BuildView(route, out var masterPage);
            Assert.IsNull(route.Exception);
            Assert.AreEqual(CompilationState.CompletedSuccessfully, route.Status);
            Assert.IsNotNull(masterPage);
            Assert.AreEqual(masterPage, service.GetMasterPages().FirstOrDefault(m => m.VirtualPath == "MasterPage.dothtml"));
            // Assert.AreEqual(CompilationState.None, masterPage.Status); // it's not deterministic, because the master page is built asynchronously after the view asks for its viewmodel type
            service.BuildView(masterPage, out _);
            Assert.AreEqual(CompilationState.CompletedSuccessfully, masterPage.Status);
        }

        [TestMethod]
        public void NonCompilable()
        {
            var route = service.GetRoutes().First(r => r.RouteName == "NonCompilable");
            Assert.AreEqual(CompilationState.NonCompilable, route.Status);
            service.BuildView(route, out _);
            Assert.AreEqual(CompilationState.NonCompilable, route.Status);
            Assert.IsNull(route.Exception);
        }
    }

    public class ControlWithContextInjection: DotvvmControl
    {
        public ControlWithContextInjection(IDotvvmRequestContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
        }
    }

    public class ControlWithUnresolvableDependency: DotvvmControl
    {
        public ControlWithUnresolvableDependency(ThisServiceIsntRegistered dependency)
        {
            if (dependency == null)
                throw new ArgumentNullException(nameof(dependency));
        }

        public class ThisServiceIsntRegistered
        {
        }
    }
}
