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
            // only names with the owner type are looked up among all properties
            if (!markupName.Contains("."))
                return;

            var owners = FindOwners(markupName, mode).Select(t => t.FullName).OrderBy(n => n).ToArray();
            if (owners.Length > 1)
            {
                nameNode.AddWarning(
                    $"The attached property '{markupName}' is ambiguous, it is declared by multiple types: {string.Join(", ", owners)}. " +
                    $"DotVVM uses the one declared by {setter.Property.DeclaringType.FullName}, which depends on the order in which the types were registered. " +
                    "Use unique names for the owner types to avoid the conflict.");
            }
        }

        /// <summary> Finds the types declaring a property or property group which the markup name can refer to. It follows the lookup in <see cref="DefaultControlResolver" />, where properties take precedence over property groups. </summary>
        private static Type[] FindOwners(string markupName, MappingMode mode)
        {
            var properties = DotvvmProperty.AllProperties
                .Where(p => p.FullName.Equals(markupName, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.DeclaringType)
                .Distinct()
                .ToArray();
            if (properties.Length > 0)
                return properties;

            var nameParts = markupName.Split('.');
            return DotvvmPropertyGroup.FindAttachedPropertyCandidates(nameParts[0])
                .Where(g => g.MarkupOptions.MappingMode.HasFlag(mode) &&
                            g.Prefixes.Any(p => nameParts[1].StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                .Select(g => g.DeclaringType)
                .Distinct()
                .ToArray();
        }
    }
}
