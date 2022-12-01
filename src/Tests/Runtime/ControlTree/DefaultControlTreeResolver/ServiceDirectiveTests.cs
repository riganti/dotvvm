using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class ServiceDirectiveTests : DefaultControlTreeResolverTestsBase
    {
        [TestMethod]
        public void ResolvedTree_ServiceDirective_CorrectBindingFromInjectedService()
        {
            var root = ParseSource(@$"
@viewModel object
@service testService = DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver.{nameof(TestService)}

<dot:Button Click={{staticCommand: testService.TestCall()}} Text=""Test"" />
");
            CheckServiceAndBinding(root);
        }

        [TestMethod]
        public void ResolvedTree_ServiceDirective_CorrectBindingFromInjectedService_UsingImportedNamespace()
        {
            var root = ParseSource(@$"
@viewModel object
@import DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver
@service testService = {nameof(TestService)}

<dot:Button Click={{staticCommand: testService.TestCall()}} Text=""Test"" />
");
            CheckServiceAndBinding(root);
        }

        [TestMethod]
        public void ResolvedTree_ServiceDirective_CorrectBindingFromInjectedService_UsingImportedAliasedNamespace()
        {
            var root = ParseSource(@$"
@viewModel object
@import testServiceAlias = DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver.{nameof(TestService)}
@service testService = testServiceAlias

<dot:Button Click={{staticCommand: testService.TestCall()}} Text=""Test"" />
");
            CheckServiceAndBinding(root);
        }

        [TestMethod]
        public void ResolvedTree_ServiceDirective_CorrectBindingFromInjectedService_UsingGlobalImportedAliasedNamespace()
        {
            configuration.Markup.ImportedNamespaces.Add(new NamespaceImport(
                $"DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver.{nameof(TestService)}",
                "testServiceAlias"));

            var root = ParseSource(@$"
@viewModel object
@service testService = testServiceAlias

<dot:Button Click={{staticCommand: testService.TestCall()}} Text=""Test"" />
");
            CheckServiceAndBinding(root);
        }

        [TestMethod]
        public void ResolvedTree_ServiceDirective_CorrectBindingFromInjectedService_UsingGlobalImportedNamespace()
        {
            configuration.Markup.ImportedNamespaces.Add(new NamespaceImport($"DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver"));

            var root = ParseSource(@$"
@viewModel object
@service testService = {nameof(TestService)}

<dot:Button Click={{staticCommand: testService.TestCall()}} Text=""Test"" />
");
            CheckServiceAndBinding(root);
        }

        private static void CheckServiceAndBinding(ResolvedTreeRoot root)
        {
            var serviceDirective = EnsureSingleResolvedServiceDirective(root);
            var binding = EnsureSingleResolvedBinding(root);

            var serviceExtensionParameters = root.DataContextTypeStack.ExtensionParameters.OfType<InjectedServiceExtensionParameter>().First();

            Assert.IsFalse(serviceDirective.DothtmlNode.HasNodeErrors);
            Assert.IsNotNull(serviceDirective.Type);
            Assert.AreEqual(serviceDirective.NameSyntax.Name, "testService");
            Assert.AreEqual(serviceDirective.Type.FullName, typeof(TestService).FullName);

            Assert.AreEqual(serviceExtensionParameters.Identifier, "testService");
            Assert.AreEqual(serviceExtensionParameters.ParameterType.FullName, typeof(TestService).FullName);

            Assert.AreEqual(typeof(void).FullName, binding.ResultType.FullName);
        }

        private static ResolvedServiceInjectDirective EnsureSingleResolvedServiceDirective(ResolvedTreeRoot root) =>
                root.Directives["service"]
                    .Single()
                    .CastTo<ResolvedServiceInjectDirective>();

        private static ResolvedBinding EnsureSingleResolvedBinding(ResolvedTreeRoot root) =>
              root.Content[2]
                   .CastTo<ResolvedControl>()
                   .Properties.First().Value.CastTo<ResolvedPropertyBinding>()
                   .Binding;
    }
}
