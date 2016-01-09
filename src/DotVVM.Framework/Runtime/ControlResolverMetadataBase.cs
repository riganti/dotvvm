using System;
using System.Collections.Generic;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.ControlTree;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Runtime
{
    public abstract class ControlResolverMetadataBase : IControlResolverMetadata
    {
        private readonly IControlType controlType;
        private readonly ControlMarkupOptionsAttribute attribute;
        protected Dictionary<string, IPropertyDescriptor> properties;

        public string Namespace => controlType.Type.Namespace;

        public string Name => controlType.Type.Name;

        public ITypeDescriptor Type => controlType.Type;

        public bool HasHtmlAttributesCollection => Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof (IControlWithHtmlAttributes)));

        public IEnumerable<string> PropertyNames => properties.Keys;

        public bool TryGetProperty(string name, out IPropertyDescriptor value)
        {
            return properties.TryGetValue(name, out value);
        }

        public bool IsContentAllowed => attribute.AllowContent;

        public IPropertyDescriptor DefaultContentProperty => !string.IsNullOrEmpty(attribute.DefaultContentProperty) ? properties[attribute.DefaultContentProperty] : null;

        public string VirtualPath => controlType.VirtualPath;

        public ITypeDescriptor DataContextConstraint => controlType.DataContextRequirement;

        public IEnumerable<IPropertyDescriptor> AllProperties => properties.Values;


        public ControlResolverMetadataBase(IControlType controlType)
        {
            this.controlType = controlType;
            this.attribute = controlType.Type.GetControlMarkupOptionsAttribute();

            this.properties = new Dictionary<string, IPropertyDescriptor>(StringComparer.CurrentCultureIgnoreCase);
        }
    }
}