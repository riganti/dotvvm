using System;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class AttachedPropertyOwnerTests
    {
        private static string[] GetWarnings(ResolvedTreeRoot tree) =>
            tree.DothtmlNode!.EnumerateNodes().SelectMany(n => n.NodeWarnings).ToArray();

        [TestMethod]
        public void UniqueAttachedProperty_ResolvesWithoutWarning()
        {
            var expected = AttachedPropertyOwners.First.DemoColumn.OnlyFirstProperty;
            _ = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;

            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.OnlyFirst='unique' />");

            Assert.IsTrue(tree.Content.Single().TryGetProperty(expected, out _));
            Assert.AreEqual(0, GetWarnings(tree).Length);
        }

        [TestMethod]
        public void AttachedPropertyOfFrameworkControl_ResolvesWithoutWarning()
        {
            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div Validation.Enabled='false' />");

            Assert.IsTrue(tree.Content.Single().TryGetProperty(Validation.EnabledProperty, out _));
            Assert.AreEqual(0, GetWarnings(tree).Length);
        }

        [TestMethod]
        public void AmbiguousAttachedProperty_StillResolvesToOneOfTheOwners()
        {
            var first = AttachedPropertyOwners.First.DemoColumn.CssClassProperty;
            var second = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;
            Assert.AreNotEqual(first.DeclaringType, second.DeclaringType);
            Assert.AreEqual(first.FullName, second.FullName);

            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.CssClass='sort-asc' />");

            var control = tree.Content.Single();
            Assert.AreEqual(1, new[] { first, second }.Count(p => control.TryGetProperty(p, out _)));
        }

        [TestMethod]
        public void AmbiguousAttachedProperty_ReportsWarningNamingAllOwners()
        {
            _ = AttachedPropertyOwners.First.DemoColumn.CssClassProperty;
            _ = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;

            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.CssClass='sort-asc' />");

            var warning = GetWarnings(tree).Single();
            StringAssert.Contains(warning, "ambiguous");
            StringAssert.Contains(warning, typeof(AttachedPropertyOwners.First.DemoColumn).FullName);
            StringAssert.Contains(warning, typeof(AttachedPropertyOwners.Second.DemoColumn).FullName);
        }

        [TestMethod]
        public void AmbiguousAttachedProperty_IsDetectedCaseInsensitively()
        {
            _ = AttachedPropertyOwners.First.DemoColumn.CssClassProperty;
            _ = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;

            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div democolumn.cssclass='sort-asc' />");

            StringAssert.Contains(GetWarnings(tree).Single(), "ambiguous");
        }

        [TestMethod]
        public void AmbiguousAttachedProperty_UsedAsInnerElement_ReportsWarning()
        {
            var first = AttachedPropertyOwners.First.DemoColumn.LabelProperty;
            var second = AttachedPropertyOwners.Second.DemoColumn.LabelProperty;

            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div><DemoColumn.Label>Name</DemoColumn.Label></div>");

            var control = tree.Content.Single();
            Assert.AreEqual(1, new[] { first, second }.Count(p => control.TryGetProperty(p, out _)));
            StringAssert.Contains(GetWarnings(tree).Single(), "ambiguous");
        }

        [TestMethod]
        public void UniqueAttachedPropertyGroup_ResolvesWithoutWarning()
        {
            var expected = AttachedPropertyOwners.First.DemoColumn.OnlyFirstValuesGroupDescriptor.GetDotvvmProperty("color");
            _ = AttachedPropertyOwners.Second.DemoColumn.ValuesGroupDescriptor;

            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.OnlyFirstValue-color='blue' />");

            Assert.IsTrue(tree.Content.Single().TryGetProperty(expected, out _));
            Assert.AreEqual(0, GetWarnings(tree).Length);
        }

        [TestMethod]
        public void AmbiguousAttachedPropertyGroup_StillResolvesAndReportsWarning()
        {
            var first = AttachedPropertyOwners.First.DemoColumn.ValuesGroupDescriptor;
            var second = AttachedPropertyOwners.Second.DemoColumn.ValuesGroupDescriptor;
            Assert.AreNotEqual(first.DeclaringType, second.DeclaringType);

            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.Value-color='blue' />");

            var control = tree.Content.Single();
            Assert.AreEqual(1, new[] { first, second }.Count(g => control.TryGetProperty(g.GetDotvvmProperty("color"), out _)));
            StringAssert.Contains(GetWarnings(tree).Single(), "ambiguous");
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

        [AttachedProperty(typeof(string))]
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public static readonly DotvvmProperty LabelProperty =
            DotvvmProperty.Register<string, DemoColumn>(() => LabelProperty, string.Empty);

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

        [AttachedProperty(typeof(string))]
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public static readonly DotvvmProperty LabelProperty =
            DotvvmProperty.Register<string, DemoColumn>(() => LabelProperty, string.Empty);

        public static readonly DotvvmPropertyGroup ValuesGroupDescriptor =
            DotvvmPropertyGroup.Register<string, DemoColumn>("Value-", "Values");
    }
}
