using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class AttachedPropertyOwnerTests
    {
        [TestMethod]
        public void UniqueAttachedProperty_ResolvesOwner()
        {
            var expected = AttachedPropertyOwners.First.DemoColumn.OnlyFirstProperty;
            _ = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;

            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.OnlyFirst='unique' />");

            Assert.IsTrue(tree.Content.Single().TryGetProperty(expected, out _));
        }

        [TestMethod]
        public void AttachedPropertyOfFrameworkControl_ResolvesOwner()
        {
            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div Validation.Enabled='false' />");

            Assert.IsTrue(tree.Content.Single().TryGetProperty(Validation.EnabledProperty, out _));
        }

        [TestMethod]
        public void AmbiguousAttachedProperty_ReportsCompilationError()
        {
            var first = AttachedPropertyOwners.First.DemoColumn.CssClassProperty;
            var second = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;
            Assert.AreNotEqual(first.DeclaringType, second.DeclaringType);
            Assert.AreEqual(first.FullName, second.FullName);

            Assert.ThrowsException<DotvvmCompilationException>(() => DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.CssClass='sort-asc' />"));
        }

        [TestMethod]
        public void AmbiguousAttachedProperty_MessageNamesAllOwners()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() => DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.CssClass='sort-asc' />"));

            StringAssert.Contains(exception.Message, "ambiguous");
            StringAssert.Contains(exception.Message, typeof(AttachedPropertyOwners.First.DemoColumn).FullName);
            StringAssert.Contains(exception.Message, typeof(AttachedPropertyOwners.Second.DemoColumn).FullName);
        }

        [TestMethod]
        public void AmbiguousAttachedProperty_IsDetectedCaseInsensitively()
        {
            _ = AttachedPropertyOwners.First.DemoColumn.CssClassProperty;
            _ = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;

            Assert.ThrowsException<DotvvmCompilationException>(() => DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div democolumn.cssclass='sort-asc' />"));
        }

        [TestMethod]
        public void UniqueAttachedPropertyGroup_ResolvesOwner()
        {
            var expected = AttachedPropertyOwners.First.DemoColumn.OnlyFirstValuesGroupDescriptor.GetDotvvmProperty("color");
            _ = AttachedPropertyOwners.Second.DemoColumn.ValuesGroupDescriptor;

            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.OnlyFirstValue-color='blue' />");

            Assert.IsTrue(tree.Content.Single().TryGetProperty(expected, out _));
        }

        [TestMethod]
        public void AmbiguousAttachedPropertyGroup_ReportsCompilationError()
        {
            var first = AttachedPropertyOwners.First.DemoColumn.ValuesGroupDescriptor;
            var second = AttachedPropertyOwners.Second.DemoColumn.ValuesGroupDescriptor;
            Assert.AreNotEqual(first.DeclaringType, second.DeclaringType);

            Assert.ThrowsException<DotvvmCompilationException>(() => DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.Value-color='blue' />"));
        }
    }
}

namespace DotVVM.Framework.Tests.Runtime.AttachedPropertyOwners.First
{
    [ContainsDotvvmProperties]
    public class DemoColumn
    {
        [AttachedProperty(typeof(string))]
        public static readonly DotvvmProperty CssClassProperty =
            DotvvmProperty.Register<string, DemoColumn>(() => CssClassProperty, string.Empty);

        [AttachedProperty(typeof(string))]
        public static readonly DotvvmProperty OnlyFirstProperty =
            DotvvmProperty.Register<string, DemoColumn>(() => OnlyFirstProperty, string.Empty);

        public static readonly DotvvmPropertyGroup ValuesGroupDescriptor =
            DotvvmPropertyGroup.Register<string, DemoColumn>("Value-", "Values");

        public static readonly DotvvmPropertyGroup OnlyFirstValuesGroupDescriptor =
            DotvvmPropertyGroup.Register<string, DemoColumn>("OnlyFirstValue-", "OnlyFirstValues");
    }
}

namespace DotVVM.Framework.Tests.Runtime.AttachedPropertyOwners.Second
{
    [ContainsDotvvmProperties]
    public class DemoColumn
    {
        [AttachedProperty(typeof(string))]
        public static readonly DotvvmProperty CssClassProperty =
            DotvvmProperty.Register<string, DemoColumn>(() => CssClassProperty, string.Empty);

        public static readonly DotvvmPropertyGroup ValuesGroupDescriptor =
            DotvvmPropertyGroup.Register<string, DemoColumn>("Value-", "Values");
    }
}
