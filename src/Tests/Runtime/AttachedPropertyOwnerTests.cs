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
        private static readonly string FirstOwner = typeof(AttachedPropertyOwners.First.DemoColumn).FullName;
        private static readonly string SecondOwner = typeof(AttachedPropertyOwners.Second.DemoColumn).FullName;

        private static string[] GetWarnings(ResolvedTreeRoot tree) =>
            tree.DothtmlNode!.EnumerateNodes().SelectMany(n => n.NodeWarnings).ToArray();

        private static string[] GetErrors(ResolvedTreeRoot tree) =>
            tree.DothtmlNode!.EnumerateNodes().SelectMany(n => n.NodeErrors).ToArray();

        private static ResolvedTreeRoot ParseDiv(string attribute) =>
            DotvvmTestHelper.ParseResolvedTree($"@viewModel System.Object\n<div {attribute} />");

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
        public void AmbiguousAttachedProperty_WarningNamesTheOwnerThatWasUsed()
        {
            var first = AttachedPropertyOwners.First.DemoColumn.CssClassProperty;
            var second = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;

            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.CssClass='sort-asc' />");

            var control = tree.Content.Single();
            var used = new[] { first, second }.Single(p => control.TryGetProperty(p, out _));
            StringAssert.Contains(GetWarnings(tree).Single(), "uses the one declared by " + used.DeclaringType.FullName);
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

        [TestMethod]
        public void AttachedPropertyGroupsWithOverlappingPrefixes_ResolveWithoutWarning()
        {
            // The markup name starts with both prefixes ("Item" and "ItemName"), but they are different names, not a conflict.
            var items = AttachedPropertyOwners.First.DemoColumn.ItemsGroupDescriptor;
            var itemNames = AttachedPropertyOwners.Second.DemoColumn.ItemNamesGroupDescriptor;
            Assert.AreNotEqual(items.DeclaringType, itemNames.DeclaringType);

            var tree = DotvvmTestHelper.ParseResolvedTree(
                "@viewModel System.Object\n<div DemoColumn.ItemNameX='blue' />");

            // the resolver takes the first prefix which matches, so either group is used
            var control = tree.Content.Single();
            Assert.AreEqual(1, new[] { items.GetDotvvmProperty("NameX"), itemNames.GetDotvvmProperty("X") }
                .Count(p => control.TryGetProperty(p, out _)));
            Assert.AreEqual(0, GetWarnings(tree).Length);
        }

        [TestMethod]
        public void HtmlAttributeWithDotInName_IsNotAnAttachedProperty()
        {
            _ = AttachedPropertyOwners.First.DemoColumn.CssClassProperty;
            _ = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;

            // the same result as for any other dotted name which is not an attached property: not resolved, no ambiguity warning
            foreach (var attribute in new[] { "data-foo.bar", "data-foo.bar.baz" })
            {
                var tree = ParseDiv($"{attribute}='x'");

                Assert.AreEqual(0, GetWarnings(tree).Length);
                StringAssert.Contains(GetErrors(tree).Single(), $"does not have a property '{attribute}'");
            }
        }

        [TestMethod]
        public void FullOwnerName_Attribute_ResolvesToTheNamedOwner()
        {
            var first = AttachedPropertyOwners.First.DemoColumn.CssClassProperty;
            var second = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;

            foreach (var (owner, expected, other) in new[] { (FirstOwner, first, second), (SecondOwner, second, first) })
            {
                var tree = ParseDiv($"{owner}.CssClass='sort-asc'");

                var control = tree.Content.Single();
                Assert.IsTrue(control.TryGetProperty(expected, out _));
                Assert.IsFalse(control.TryGetProperty(other, out _));
                Assert.AreEqual(0, GetWarnings(tree).Length);
            }
        }

        [TestMethod]
        public void FullOwnerName_OfUniqueProperty_ResolvesWithoutWarning()
        {
            var expected = AttachedPropertyOwners.First.DemoColumn.OnlyFirstProperty;
            _ = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;

            var tree = ParseDiv($"{FirstOwner}.OnlyFirst='unique'");

            Assert.IsTrue(tree.Content.Single().TryGetProperty(expected, out _));
            Assert.AreEqual(0, GetWarnings(tree).Length);
        }

        [TestMethod]
        public void FullOwnerName_IsCaseInsensitive()
        {
            var first = AttachedPropertyOwners.First.DemoColumn.CssClassProperty;
            var second = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;

            var tree = ParseDiv($"{SecondOwner.ToLowerInvariant()}.cssclass='sort-asc'");

            var control = tree.Content.Single();
            Assert.IsTrue(control.TryGetProperty(second, out _));
            Assert.IsFalse(control.TryGetProperty(first, out _));
            Assert.AreEqual(0, GetWarnings(tree).Length);
        }

        [TestMethod]
        public void FullOwnerName_InnerElement_ResolvesToTheNamedOwner()
        {
            var first = AttachedPropertyOwners.First.DemoColumn.LabelProperty;
            var second = AttachedPropertyOwners.Second.DemoColumn.LabelProperty;

            foreach (var (owner, expected, other) in new[] { (FirstOwner, first, second), (SecondOwner, second, first) })
            {
                var tree = DotvvmTestHelper.ParseResolvedTree(
                    $"@viewModel System.Object\n<div><{owner}.Label>Name</{owner}.Label></div>");

                var control = tree.Content.Single();
                Assert.IsTrue(control.TryGetProperty(expected, out _));
                Assert.IsFalse(control.TryGetProperty(other, out _));
                Assert.AreEqual(0, GetWarnings(tree).Length);
            }
        }

        [TestMethod]
        public void FullOwnerName_PropertyGroup_ResolvesToTheNamedOwner()
        {
            var first = AttachedPropertyOwners.First.DemoColumn.ValuesGroupDescriptor.GetDotvvmProperty("color");
            var second = AttachedPropertyOwners.Second.DemoColumn.ValuesGroupDescriptor.GetDotvvmProperty("color");

            foreach (var (owner, expected, other) in new[] { (FirstOwner, first, second), (SecondOwner, second, first) })
            {
                var tree = ParseDiv($"{owner}.Value-color='blue'");

                var control = tree.Content.Single();
                Assert.IsTrue(control.TryGetProperty(expected, out _));
                Assert.IsFalse(control.TryGetProperty(other, out _));
                Assert.AreEqual(0, GetWarnings(tree).Length);
            }
        }

        [TestMethod]
        public void FullOwnerName_PropertyGroupsWithOverlappingPrefixes_ResolvesToTheNamedOwner()
        {
            var firstItem = AttachedPropertyOwners.First.DemoColumn.ItemsGroupDescriptor.GetDotvvmProperty("NameX");
            var secondItem = AttachedPropertyOwners.Second.DemoColumn.ItemNamesGroupDescriptor.GetDotvvmProperty("X");

            var first = ParseDiv($"{FirstOwner}.ItemNameX='blue'").Content.Single();
            Assert.IsTrue(first.TryGetProperty(firstItem, out _));
            Assert.IsFalse(first.TryGetProperty(secondItem, out _));

            var second = ParseDiv($"{SecondOwner}.ItemNameX='blue'").Content.Single();
            Assert.IsTrue(second.TryGetProperty(secondItem, out _));
            Assert.IsFalse(second.TryGetProperty(firstItem, out _));
        }

        [TestMethod]
        public void FullOwnerName_OfUnknownType_IsNotResolved()
        {
            var first = AttachedPropertyOwners.First.DemoColumn.CssClassProperty;
            var second = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;
            var unknownOwner = typeof(AttachedPropertyOwners.First.DemoColumn).Namespace + ".Missing.DemoColumn";

            // the namespace is right, but the type is not there / the type is there, but not the member
            foreach (var attribute in new[] { $"{unknownOwner}.CssClass", $"{FirstOwner}.Missing", $"{unknownOwner}.Value-color" })
            {
                var tree = ParseDiv($"{attribute}='x'");

                var control = tree.Content.Single();
                Assert.IsFalse(control.TryGetProperty(first, out _));
                Assert.IsFalse(control.TryGetProperty(second, out _));
                Assert.AreEqual(0, GetWarnings(tree).Length);
                StringAssert.Contains(GetErrors(tree).Single(), $"does not have a property '{attribute}'");
            }

            // an inner element with an unknown owner is just an HTML element, as before
            var element = DotvvmTestHelper.ParseResolvedTree(
                $"@viewModel System.Object\n<div><{unknownOwner}.Label>Name</{unknownOwner}.Label></div>");
            Assert.IsFalse(element.Content.Single().TryGetProperty(AttachedPropertyOwners.First.DemoColumn.LabelProperty, out _));
            Assert.IsFalse(element.Content.Single().TryGetProperty(AttachedPropertyOwners.Second.DemoColumn.LabelProperty, out _));
        }

        [TestMethod]
        public void AmbiguousAttachedProperty_WarningSuggestsTheFullNames()
        {
            _ = AttachedPropertyOwners.First.DemoColumn.CssClassProperty;
            _ = AttachedPropertyOwners.Second.DemoColumn.CssClassProperty;

            var warning = GetWarnings(ParseDiv("DemoColumn.CssClass='sort-asc'")).Single();

            StringAssert.Contains(warning, $"'{FirstOwner}.CssClass'");
            StringAssert.Contains(warning, $"'{SecondOwner}.CssClass'");
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

        public static readonly DotvvmPropertyGroup ItemsGroupDescriptor =
            DotvvmPropertyGroup.Register<string, DemoColumn>("Item", "Items");
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

        public static readonly DotvvmPropertyGroup ItemNamesGroupDescriptor =
            DotvvmPropertyGroup.Register<string, DemoColumn>("ItemName", "ItemNames");
    }
}
