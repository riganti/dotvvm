using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using Newtonsoft.Json;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public abstract class ControlResolverMetadataBase : IControlResolverMetadata
    {
        private readonly IControlType controlType;
        private readonly ControlMarkupOptionsAttribute attribute;

        private readonly Lazy<Dictionary<string, IPropertyDescriptor>> properties;
        public Dictionary<string, IPropertyDescriptor> Properties => properties.Value;


        public string Namespace => controlType.Type.Namespace;

        public string Name => controlType.Type.Name;

        [JsonIgnore]
        public ITypeDescriptor Type => controlType.Type;

        public bool HasHtmlAttributesCollection => Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof (IControlWithHtmlAttributes)));

        [JsonIgnore]
        public IEnumerable<string> PropertyNames => Properties.Keys;

        public bool TryGetProperty(string name, out IPropertyDescriptor value)
        {
            return Properties.TryGetValue(name, out value);
        }

        public bool IsContentAllowed => attribute?.AllowContent ?? true;

        [JsonIgnore]
        public IPropertyDescriptor DefaultContentProperty
        {
            get
            {
                if (string.IsNullOrEmpty(attribute?.DefaultContentProperty))
                {
                    return null;
                }

                IPropertyDescriptor result;
                return Properties.TryGetValue(attribute?.DefaultContentProperty, out result) ? result : null;
            }
        }

        public string DefaultContentPropertyName => attribute?.DefaultContentProperty;

        [JsonIgnore]
        public string VirtualPath => controlType?.VirtualPath;

        [JsonIgnore]
        public ITypeDescriptor DataContextConstraint => controlType?.DataContextRequirement;

        [JsonIgnore]
        public IEnumerable<IPropertyDescriptor> AllProperties => Properties.Values;

        [JsonIgnore]
        public abstract DataContextChangeAttribute[] DataContextChangeAttributes { get; } 

        public ControlResolverMetadataBase(IControlType controlType)
        {
            this.controlType = controlType;
            this.attribute = controlType?.Type?.GetControlMarkupOptionsAttribute();

            this.properties = new Lazy<Dictionary<string, IPropertyDescriptor>>(() => {
                var result = new Dictionary<string, IPropertyDescriptor>(StringComparer.CurrentCultureIgnoreCase);
                LoadProperties(result);
                return result;
            });
        }

        protected abstract void LoadProperties(Dictionary<string, IPropertyDescriptor> result);
    }
}