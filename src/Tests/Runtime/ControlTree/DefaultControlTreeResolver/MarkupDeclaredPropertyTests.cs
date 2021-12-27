using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Utils;
using System;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class MarkupDeclaredPropertyTests : DefaultControlTreeResolverTestsBase
    {
        [TestMethod]
        public void ResolvedTree_MarkupDeclaredProperty_PropertyCorrectTypeAndName()
        {
            var root = ParseSource(@"@viewModel object
@property string Heading
",
fileName: "control.dotcontrol");

            var declarationDirective = EnsureSingleResolvedDeclarationDirective(root);

            Assert.IsFalse(declarationDirective.DothtmlNode.HasNodeErrors);
            Assert.AreEqual(typeof(string).FullName, declarationDirective.PropertyType.FullName);
            Assert.AreEqual("Heading", declarationDirective.NameSyntax.Name);
        }

        [DataTestMethod]
        [DataRow("true", "bool", typeof(bool), true)]
        [DataRow("30", "int", typeof(int), 30)]
        [DataRow("10", "long", typeof(long), 10)]
        [DataRow("3.14", "double", typeof(double), 3.14)]
        [DataRow("'a'", "char", typeof(char), 'a')]
        public void ResolvedTree_MarkupDeclaredProperty_CorrectConstantInitialValue(string initializer, string typeName, Type propertyType, object? testedValue)
        {
            var root = ParseSource(@$"@viewModel object
@property {typeName} IsClosed = {initializer}
",
fileName: "control.dotcontrol");
            var declarationDirective = EnsureSingleResolvedDeclarationDirective(root);

            Assert.IsFalse(declarationDirective.DothtmlNode.HasNodeErrors);
            Assert.AreEqual(propertyType.FullName, declarationDirective.PropertyType.FullName);
            Assert.AreEqual("IsClosed", declarationDirective.NameSyntax.Name);

            Assert.AreEqual(testedValue, declarationDirective.InitialValue);
        }

        [TestMethod]
        public void ResolvedTree_MarkupDeclaredProperty_GuidInitializer()
        {
            var root = ParseSource(@$"@viewModel object
@property System.Guid ItemId = ""645f970d-2879-4fff-a19d-ba7b4c4a4853""
",
fileName: "control.dotcontrol");
            var declarationDirective = EnsureSingleResolvedDeclarationDirective(root);

            Assert.IsFalse(declarationDirective.DothtmlNode.HasNodeErrors);
            Assert.AreEqual(typeof(Guid).FullName, declarationDirective.PropertyType.FullName);
            Assert.AreEqual("ItemId", declarationDirective.NameSyntax.Name);

            Assert.AreEqual(new Guid("645f970d-2879-4fff-a19d-ba7b4c4a4853"), declarationDirective.InitialValue);
        }

        private static ResolvedPropertyDeclarationDirective EnsureSingleResolvedDeclarationDirective(ResolvedTreeRoot root) =>
                    root.Directives["property"]
                        .Single()
                        .CastTo<ResolvedPropertyDeclarationDirective>();
    }
}
