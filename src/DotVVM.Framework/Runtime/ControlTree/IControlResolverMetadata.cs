using System.Collections.Generic;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IControlResolverMetadata
    {
        ITypeDescriptor Type { get; }

        bool HasHtmlAttributesCollection { get; }

        IEnumerable<string> PropertyNames { get; }

        bool TryGetProperty(string name, out IPropertyDescriptor value);

        bool IsContentAllowed { get; }

        IPropertyDescriptor DefaultContentProperty { get; }

        string VirtualPath { get; }

        ITypeDescriptor DataContextConstraint { get; }

        IEnumerable<IPropertyDescriptor> AllProperties { get; }
    }
}