using System;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation
{
    /// <summary> Warns when an attached property is referenced by an owner type name that more than one type has. The markup only contains the type name without namespace (OwnerTypeName.Property), so the types can't be told apart and DotVVM uses one of them. </summary>
    public class AmbiguousAttachedPropertyVisitor : ResolvedControlTreeVisitor
    {
        public override void DefaultVisit(IResolvedTreeNode node)
        {
            base.DefaultVisit(node);
            if (node is not ResolvedPropertySetter { DothtmlNode: {} markupNode } setter)
                return;

            switch (markupNode)
            {
                case DothtmlAttributeNode attribute:
                    CheckName(setter, attribute.AttributeFullName, attribute.AttributeNameNode, MappingMode.Attribute);
                    break;
                case DothtmlElementNode element when string.IsNullOrEmpty(element.TagPrefix):
                    CheckName(setter, element.TagName, element.TagNameNode, MappingMode.InnerElement);
                    break;
            }
        }

        private static void CheckName(ResolvedPropertySetter setter, string markupName, DothtmlNode nameNode, MappingMode mode)
        {
            // Only the names starting with the short name of the owner type can be ambiguous.
            // When the owner is written with its namespace, it is exactly one type.
            if (!markupName.StartsWith(setter.Property.DeclaringType.Name + ".", StringComparison.OrdinalIgnoreCase))
                return;

            var owners = FindOwners(setter, markupName, mode).Select(t => t.FullName).OrderBy(n => n).ToArray();
            if (owners.Length > 1)
            {
                var memberPart = markupName.Substring(markupName.IndexOf('.'));
                nameNode.AddWarning(
                    $"The attached property '{markupName}' is ambiguous, it is declared by multiple types: {string.Join(", ", owners)}. " +
                    $"DotVVM uses the one declared by {setter.Property.DeclaringType.FullName}. " +
                    $"To select the owner explicitly, use its full name ({string.Join(" or ", owners.Select(o => "'" + o + memberPart + "'"))}), " +
                    "or use unique names for the owner types to avoid the conflict.");
            }
        }

        /// <summary> Finds the types declaring a property or property group member with the same name as the one the markup name was resolved to. </summary>
        private static Type[] FindOwners(ResolvedPropertySetter setter, string markupName, MappingMode mode)
        {
            if (setter.Property is GroupedDotvvmProperty member)
            {
                // The markup name is the owner type, the prefix of the group and the name of the group member.
                // Groups declare the same name only when their prefix is the same: a group with a longer or shorter
                // prefix is a different name, even though the markup name also starts with that prefix.
                var nameParts = markupName.Split('.');
                var candidates = DotvvmPropertyGroup.FindAttachedPropertyCandidates(nameParts[0]).ToArray();
                var prefixLength = nameParts[1].Length - member.GroupMemberName.Length;
                if (prefixLength < 0 || !candidates.Contains(member.PropertyGroup))
                {
                    // not an attached property group, the dot is just a part of the attribute name (for example data-foo.bar)
                    return Array.Empty<Type>();
                }

                var usedPrefix = nameParts[1].Substring(0, prefixLength);
                return candidates
                    .Where(g => g.MarkupOptions.MappingMode.HasFlag(mode) &&
                                g.Prefixes.Any(p => p.Equals(usedPrefix, StringComparison.OrdinalIgnoreCase)))
                    .Select(g => g.DeclaringType)
                    .Distinct()
                    .ToArray();
            }

            return DotvvmProperty.AllProperties
                .Where(p => p.FullName.Equals(markupName, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.DeclaringType)
                .Distinct()
                .ToArray();
        }
    }
}
