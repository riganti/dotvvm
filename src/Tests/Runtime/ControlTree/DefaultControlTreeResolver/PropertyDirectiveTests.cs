using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree;

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

            AssertEx.BindingNode(property.Attributes[0].Initializer, "", 19, 0);
            AssertEx.BindingNode(property.Attributes[1].Initializer, "", 28, 0);
            AssertEx.BindingNode(property.Attributes[2].Initializer, "", 42, 1, true);
            AssertEx.BindingNode(property.Attributes[3].Initializer, "t", 104, 2);
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
        public void ResolvedTree_PropertyDirectiveInvalidArrayInicializer_ResolvedCorrectly()
        {
            var root = ParseSource(@$"
@viewModel object
@property string a=[, MarkupOptionsAttribute.Required = true");

            var property = root.Directives["property"].SingleOrDefault() as IAbstractPropertyDeclarationDirective;

            Assert.AreEqual("System.String", property.PropertyType.FullName);
            Assert.AreEqual("a", property.NameSyntax.Name);

            var nodes = property.InitializerSyntax.EnumerateChildNodes();
            Assert.IsFalse(nodes.Any(n => n == property.InitializerSyntax));
        }

        [TestMethod]
        public void ResolvedTree_PropertyDirectiveArrayInicializerAndAttributes_ResolvedCorrectly()
        {
            var root = ParseSource("""
@viewModel object
@property string[] MyProperty=["",""], MarkupOptionsAttribute.Required = true, MarkupOptionsAttribute.AllowBinding = false
""");

            var property = root.Directives["property"].SingleOrDefault() as IAbstractPropertyDeclarationDirective;

            Assert.AreEqual("System.String[]", property.PropertyType.FullName);
            Assert.AreEqual("MyProperty", property.NameSyntax.Name);

            Assert.AreEqual(2, property.Attributes.Count);
            AssertEx.BindingNode(property.Attributes[0].Initializer, "True", 62, 5);
            AssertEx.BindingNode(property.Attributes[1].Initializer, "False", 106, 6);
        }

        [TestMethod]
        public void ResolvedTree_PropertyDirectiveSameName_ErrorReported()
        {
            var root = ParseSource("""
                @viewModel object
                @property int MyProperty
                @property bool MyProperty
                @property string MyProperty

                {{value: _control.MyProperty}}
""", "control.dotcontrol");

            var firstProperty = root.Directives["property"][0] as IAbstractPropertyDeclarationDirective;
            var secondProperty = root.Directives["property"][1] as IAbstractPropertyDeclarationDirective;
            var thirdProperty = root.Directives["property"][2] as IAbstractPropertyDeclarationDirective;

            var property = root.Metadata.FindProperty("MyProperty");

            //First property wins
            Assert.AreEqual(property.PropertyType, typeof(int));

            Assert.IsFalse(firstProperty.DothtmlNode.HasNodeErrors);
            Assert.IsTrue(secondProperty.DothtmlNode.HasNodeErrors);
            Assert.IsTrue(thirdProperty.DothtmlNode.HasNodeErrors);


        }
    }
}
