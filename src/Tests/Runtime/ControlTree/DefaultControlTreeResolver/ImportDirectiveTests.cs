﻿using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Tests.Runtime.ControlTree.TestInternalOnlyNamespace
{
    class InternalClass { }
}

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class ImportDirectiveTests : DefaultControlTreeResolverTestsBase
    {
        [TestMethod]
        public void ResolvedTree_ImportDirective_CorrectBindingTypeFromImportedNamespace()
        {
            var root = ParseSource(@"
@viewModel object
@import System

{{value: StringComparison.OrdinalIgnoreCase}}
");
            var importDirective = EnsureSingleResolvedImportDirective(root);
            var binding = EnsureSingleResolvedBinding(root);

            var namespaceImport = root.DataContextTypeStack.NamespaceImports.First();

            Assert.IsFalse(importDirective.HasError);
            Assert.IsNull(importDirective.Type);
            Assert.IsTrue(importDirective.IsNamespace);

            Assert.AreEqual(namespaceImport.Namespace, "System");

            Assert.IsFalse(binding.Errors.HasErrors);
            Assert.AreEqual(typeof(StringComparison).FullName, binding.ResultType.FullName);
        }

        [TestMethod]
        public void ResolvedTree_ImportDirective_CorrectSimpleTypeAlias()
        {
            var root = ParseSource(@"
@viewModel object
@import styles = System.Globalization.DateTimeStyles

{{value: styles.AdjustToUniversal}}
");
            var importDirective = EnsureSingleResolvedImportDirective(root);
            var binding = EnsureSingleResolvedBinding(root);

            Assert.IsFalse(importDirective.HasError);
            Assert.IsNotNull(importDirective.Type);
            Assert.IsFalse(importDirective.IsNamespace);
            Assert.AreEqual(typeof(System.Globalization.DateTimeStyles).FullName, importDirective.Type.FullName);

            Assert.IsFalse(binding.Errors.HasErrors);
            Assert.AreEqual(typeof(System.Globalization.DateTimeStyles).FullName, binding.ResultType.FullName);
        }

        [TestMethod]
        public void ResolvedTree_ImportDirective_CorrectGenericTypeAlias()
        {
            var root = ParseSource(@"
@viewModel object
@import list = System.Collections.Generic.List<System.Int32>
");
            var importDirective = EnsureSingleResolvedImportDirective(root);

            Assert.IsFalse(importDirective.HasError);
            Assert.IsNotNull(importDirective.Type);
            Assert.IsFalse(importDirective.IsNamespace);
            Assert.AreEqual(typeof(List<int>).FullName, importDirective.Type.FullName);
        }

        [TestMethod]
        public void ResolvedTree_ImportDirective_NamespaceDoesNotExist()
        {
            var root = ParseSource(@"
@viewModel object
@import This.Namespace.Does.Not.Exist
");
            var importDirective = EnsureSingleResolvedImportDirective(root);

            Assert.IsTrue(importDirective.HasError);
            Assert.AreEqual("This.Namespace.Does.Not.Exist is unknown type or namespace.", importDirective.DothtmlNode.NodeErrors.Single());
        }
        [TestMethod]
        public void ResolvedTree_ImportDirective_NamespaceDoesNotExist_Alias()
        {
            var root = ParseSource(@"
@viewModel object
@import testns = This.Namespace.Does.Not.Exist
");
            var importDirective = EnsureSingleResolvedImportDirective(root);

            Assert.IsTrue(importDirective.HasError);
            Assert.AreEqual("This.Namespace.Does.Not.Exist is unknown type or namespace.", importDirective.DothtmlNode.NodeErrors.Single());
        }

        [TestMethod]
        public void ResolvedTree_ImportDirective_InternalNamespace()
        {
            var root = ParseSource(@"
@viewModel object
@import DotVVM.Framework.Tests.Runtime.ControlTree.TestInternalOnlyNamespace
");
            var importDirective = EnsureSingleResolvedImportDirective(root);

            Assert.IsFalse(importDirective.HasError, message: importDirective.DothtmlNode.NodeErrors.SingleOrDefault());
            Assert.IsNull(importDirective.Type);
            Assert.IsTrue(importDirective.IsNamespace);
        }


        private static ResolvedImportDirective EnsureSingleResolvedImportDirective(ResolvedTreeRoot root) =>
                root.Directives["import"]
                    .Single()
                    .CastTo<ResolvedImportDirective>();

        private static ResolvedBinding EnsureSingleResolvedBinding(ResolvedTreeRoot root) =>
              root.Content[2]
                   .CastTo<ResolvedControl>()
                   .Properties.First().Value.CastTo<ResolvedPropertyBinding>()
                   .Binding;
    }
}
