using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public static class ResolvedTreeHelpers
    {
        public static ResolvedPropertySetter? GetValue(this ResolvedControl control, DotvvmProperty property) =>
            control.Properties.TryGetValue(property, out var value) ? value : null;
        
        public static Type GetResultType(this ResolvedPropertySetter property) =>
           (property is ResolvedPropertyBinding binding ? ResolvedTypeDescriptor.ToSystemType(binding.Binding.ResultType) :
            property is ResolvedPropertyValue value ? value.Value?.GetType() :
            property is ResolvedPropertyControl control ? control.Control?.Metadata.Type :
            null) ?? property.Property.PropertyType;

        public static object? GetValue(this ResolvedPropertySetter setter) =>
            setter switch {
                ResolvedPropertyValue value => value.Value,
                ResolvedPropertyBinding binding => binding.Binding.Binding,
                ResolvedPropertyTemplate value => value.Content,
                ResolvedPropertyControl value => value.Control,
                ResolvedPropertyControlCollection value => value.Controls,
                ResolvedPropertyCapability value => value.ToCapabilityObject(throwExceptions: false),
                _ => throw new NotSupportedException()
            };

        public static DataContextStack GetDataContextStack(this ResolvedPropertySetter setter, ResolvedControl? parentControl) =>
            setter switch {
                ResolvedPropertyBinding binding => binding.Binding.DataContextTypeStack,
                ResolvedPropertyTemplate { Content: { Count: > 0 } } value => value.Content.First().DataContextTypeStack,
                ResolvedPropertyControl { Control: {} } value => value.Control.DataContextTypeStack,
                ResolvedPropertyControlCollection { Controls: { Count: > 0 } } value => value.Controls.First().DataContextTypeStack,
                _ => setter.Property.GetDataContextType(parentControl ?? setter.ParentControl().NotNull("Could not get data context type from property setter without a parent control."))
            };

        public static ResolvedControl? ParentControl(this ResolvedTreeNode node) =>
            node.Parent switch {
                ResolvedControl control => control,
                null => null,
                var parentNode => parentNode.ParentControl(),
            };

        
        public static bool IsOnlyWhitespace(this IAbstractControl control) =>
            control.Metadata.Type.IsEqualTo(ResolvedTypeDescriptor.Create(typeof(RawLiteral))) && control.DothtmlNode?.IsNotEmpty() == false;

        public static bool HasOnlyWhiteSpaceContent(this IAbstractContentNode control) =>
            control.Content.All(IsOnlyWhitespace);
    }
}
