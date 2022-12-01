using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IControlResolverMetadata
    {
        ITypeDescriptor Type { get; }

        bool HasHtmlAttributesCollection { get; }

        IEnumerable<string> PropertyNames { get; }

        bool TryGetProperty(string name, [NotNullWhen(true)] out IPropertyDescriptor? value);

        bool IsContentAllowed { get; }

        IPropertyDescriptor? DefaultContentProperty { get; }

        string? VirtualPath { get; }

        ITypeDescriptor? DataContextConstraint { get; }

        IEnumerable<IPropertyDescriptor> AllProperties { get; }
		
		/// <summary>
		/// Gets property groups available on this control (list is ordered - longer prefix goes first)
		/// </summary>
		IReadOnlyList<PropertyGroupMatcher> PropertyGroups { get; }

        ControlPrecompilationMode PrecompilationMode { get; }

        DataContextChangeAttribute[] DataContextChangeAttributes { get; }
		DataContextStackManipulationAttribute? DataContextManipulationAttribute { get; }
	}
}
