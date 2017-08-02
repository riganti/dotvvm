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
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public static class ResolvedTreeHelpers
    {
        public static ResolvedPropertySetter GetValue(this ResolvedControl control, DotvvmProperty property) =>
            control.Properties.TryGetValue(property, out var value) ? value : null;
        
        public static Type GetResultType(this ResolvedPropertySetter property) =>
            property is ResolvedPropertyBinding binding ? ResolvedTypeDescriptor.ToSystemType(binding.Binding.ResultType) :
            property is ResolvedPropertyValue value ? value.Value?.GetType() ?? property.Property.PropertyType :
            property is ResolvedPropertyControl control ? control.Control.Metadata.Type :
            property.Property.PropertyType;
    }
}