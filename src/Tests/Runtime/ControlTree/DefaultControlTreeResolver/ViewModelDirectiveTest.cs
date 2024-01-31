using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Testing;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class ViewModelDirectiveTest : DefaultControlTreeResolverTestsBase
    {
        [TestMethod]
        public void ResolvedTree_MissingViewModelDirective()
        {
            var root = ParseSource(@"");

            Assert.IsTrue(root.DothtmlNode.HasNodeErrors);
            Assert.IsTrue(root.DothtmlNode.NodeErrors.First().Contains("missing"));
        }

        [TestMethod]
        public void ResolvedTree_UnknownViewModelType()
        {
            var root = ParseSource(@"@viewModel invalid
");

            var directiveNode = ((DothtmlRootNode)root.DothtmlNode).Directives.First();
            Assert.IsTrue(directiveNode.HasNodeErrors);
            Assert.IsTrue(directiveNode.NodeErrors.First().Contains("Could not resolve type"));
        }

        [TestMethod]
        public void ResolvedTree_EmptyViewModelType()
        {
            var root = ParseSource(@"@viewModel
");

            var directiveNode = ((DothtmlRootNode)root.DothtmlNode).Directives.First();
            Assert.IsTrue(directiveNode.HasNodeErrors);
        }

        [TestMethod]
        public void ResolvedTree_ViewModel_GenericType()
        {
            var root = ParseSource(@"@viewModel System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String, System.Int32>>");
            Assert.AreEqual(typeof(List<Dictionary<string, int>>), root.DataContextTypeStack.DataContextType);
        }

        [TestMethod]
        public void ResolvedTree_ViewModel_InvalidAssemblyQualified()
        {
            var root = ParseSource(@"@viewModel System.String, whatever");
            Assert.IsTrue(root.Directives.Any(d => d.Value.Any(dd => dd.DothtmlNode.HasNodeErrors)));
            Assert.AreEqual(typeof(UnknownTypeSentinel), root.DataContextTypeStack.DataContextType);
        }

        [TestMethod]
        public void ResolvedTree_ViewModel_TypeFromImportedNamespace()
        {
            var root = ParseSource(@"
@import System.Collections.Generic
@viewModel List<Dictionary<System.String, System.Int32>>
");
            Assert.IsFalse(root.Directives.Any(d => d.Value.Any(dd => dd.DothtmlNode.HasNodeErrors)));
            Assert.AreEqual(typeof(List<Dictionary<string, int>>), root.DataContextTypeStack.DataContextType);
        }

        [TestMethod]
        public void ResolvedTree_ViewModel_TypeFromImportedAliasedType()
        {
            var root = ParseSource(@"
@import viewModelAlias = DotVVM.Framework.Tests.Runtime.ControlTree.TestViewModel
@viewModel viewModelAlias
");
            Assert.IsFalse(root.Directives.Any(d => d.Value.Any(dd => dd.DothtmlNode.HasNodeErrors)));
            Assert.AreEqual(typeof(TestViewModel), root.DataContextTypeStack.DataContextType);
        }

        [TestMethod]
        public void ResolvedTree_ViewModel_TypeFromGlobalImportedAliasedType()
        {
            var config = DotvvmTestHelper.CreateConfiguration();
            config.Markup.ImportedNamespaces.Add(new NamespaceImport("DotVVM.Framework.Tests.Runtime.ControlTree.TestViewModel", "viewModelAlias"));

            var root = DotvvmTestHelper.ParseResolvedTree(@"@viewModel viewModelAlias", configuration: config);

            Assert.IsFalse(root.Directives.Any(d => d.Value.Any(dd => dd.DothtmlNode.HasNodeErrors)));
            Assert.AreEqual(typeof(TestViewModel), root.DataContextTypeStack.DataContextType);
        }

        [TestMethod]
        public void ResolvedTree_ViewModel_TypeFromGlobalImportedNamespace()
        {
            var config = DotvvmTestHelper.CreateConfiguration();
            config.Markup.ImportedNamespaces.Add(new NamespaceImport("DotVVM.Framework.Tests.Runtime.ControlTree"));

            var root = DotvvmTestHelper.ParseResolvedTree(@"@viewModel TestViewModel", configuration: config);

            Assert.IsFalse(root.Directives.Any(d => d.Value.Any(dd => dd.DothtmlNode.HasNodeErrors)));
            Assert.AreEqual(typeof(TestViewModel), root.DataContextTypeStack.DataContextType);
        }

        [TestMethod]
        [DataRow("@viewModel")]
        [DataRow("@viewmodel")]
        public void ResolvedTree_ViewModel_DirectiveIdentifier_CaseInsensitivity(string directive)
        {
            var root = ParseSource($"{directive} System.String");

            Assert.IsFalse(root.Directives.Any(d => d.Value.Any(dd => dd.DothtmlNode.HasNodeErrors)));
            Assert.AreEqual(typeof(string), root.DataContextTypeStack.DataContextType);
        }
    }
}
