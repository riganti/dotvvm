using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class ServiceDirectiveTests : DefaultControlTreeResolverTestsBase
    {
        [TestMethod]
        public void ResolvedTree_ImportDirective_CorrectBindingTypeFromImportedNamespace()
        {
            var root = ParseSource(@$"
@viewModel object
@service testService = DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver.{nameof(TestService)}

<dot:Button Click={{staticCommand: testService.TestCall()}} Text=""Test"" />
");
            var serviceDirective = EnsureSingleResolvedImportDirective(root);
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

        private static ResolvedServiceInjectDirective EnsureSingleResolvedImportDirective(ResolvedTreeRoot root) =>
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
