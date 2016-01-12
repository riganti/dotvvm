using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public abstract class ControlResolverMetadataBase : IControlResolverMetadata
    {
        private readonly IControlType controlType;
        private readonly ControlMarkupOptionsAttribute attribute;

        private readonly Lazy<Dictionary<string, IPropertyDescriptor>> properties;
        public Dictionary<string, IPropertyDescriptor> Properties => properties.Value;


        public string Namespace => controlType.Type.Namespace;

        public string Name => controlType.Type.Name;

        public ITypeDescriptor Type => controlType.Type;

        public bool HasHtmlAttributesCollection => Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof (IControlWithHtmlAttributes)));

        public IEnumerable<string> PropertyNames => Properties.Keys;

        public bool TryGetProperty(string name, out IPropertyDescriptor value)
        {
            return Properties.TryGetValue(name, out value);
        }

        public bool IsContentAllowed => attribute.AllowContent;

        public IPropertyDescriptor DefaultContentProperty => !string.IsNullOrEmpty(attribute.DefaultContentProperty) ? Properties[attribute.DefaultContentProperty] : null;

        public string VirtualPath => controlType.VirtualPath;

        public ITypeDescriptor DataContextConstraint => controlType.DataContextRequirement;

        public IEnumerable<IPropertyDescriptor> AllProperties => Properties.Values;

        public abstract DataContextChangeAttribute[] DataContextChangeAttributes { get; } 

        public ControlResolverMetadataBase(IControlType controlType)
        {
            this.controlType = controlType;
            this.attribute = controlType.Type.GetControlMarkupOptionsAttribute();

            this.properties = new Lazy<Dictionary<string, IPropertyDescriptor>>(() => {
                var result = new Dictionary<string, IPropertyDescriptor>(StringComparer.CurrentCultureIgnoreCase);
                LoadProperties(result);
                return result;
            });
        }

        protected abstract void LoadProperties(Dictionary<string, IPropertyDescriptor> result);
    }
}