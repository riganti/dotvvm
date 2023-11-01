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
    }
}
