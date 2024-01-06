using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class PropertyDirectiveTests : DefaultControlTreeResolverTestsBase
    {
        [TestMethod]
        public void ResolvedTree_PropertyDirectiveHalfWritten_AttributeResolved()
        {
            var root = ParseSource(@$"
@viewModel object
@property string MyProperty, , DotVVM., DotVVM.Fra = , DotVVM.Framework.Controls.MarkupOptionsAttribute.Required = t");

            var property = root.Directives["property"].SingleOrDefault() as IAbstractPropertyDeclarationDirective;

            Assert.AreEqual("System.String", property.PropertyType.FullName);
            Assert.AreEqual("MyProperty", property.NameSyntax.Name);

            Assert.AreEqual(4, property.Attributes.Count);
        }

        [TestMethod]
        public void ResolvedTree_PropertyDirectiveIncompatibleType_ErrorReported()
        {
            var root = ParseSource(@$"
@viewModel object
@property string MyProperty, DotVVM.Framework.Controls.MarkupOptionsAttribute.Required = 1");

            var property = root.Directives["property"].SingleOrDefault() as IAbstractPropertyDeclarationDirective;

            Assert.AreEqual("System.String", property.PropertyType.FullName);
            Assert.AreEqual("MyProperty", property.NameSyntax.Name);

            Assert.AreEqual(1, property.Attributes.Count);
            Assert.AreEqual(1, property.DothtmlNode.NodeErrors.Count());

            var error = property.DothtmlNode.NodeErrors.First();

            Assert.IsTrue(error.Contains("Cannot assign"), "Expected error about failed assignment into the attribute property.");
        }

        [TestMethod]
        public void ResolvedTree_PropertyDirectiveArrayInicializer_ResolvedCorrectly()
        {
            var root = ParseSource(@$"
@viewModel object
@property string a=[, MarkupOptionsAttribute.Required = true");

            var property = root.Directives["property"].SingleOrDefault() as IAbstractPropertyDeclarationDirective;

            var nodes = property.InitializerSyntax.EnumerateChildNodes();

            Assert.IsFalse(nodes.Any(n => n == property.InitializerSyntax));

            Assert.AreEqual(2,nodes.Count());
        }
    }
}
