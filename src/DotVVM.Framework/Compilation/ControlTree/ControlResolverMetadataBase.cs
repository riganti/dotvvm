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
        public IReadOnlyDictionary<string, IPropertyDescriptor> Properties => properties.Value;

        private readonly Lazy<List<PropertyGroupMatcher>> _propertyGroups;
        public IReadOnlyList<PropertyGroupMatcher> PropertyGroups => _propertyGroups.Value;


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

        public bool IsContentAllowed => (attribute?.AllowContent ?? true) && Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof(IDotvvmControlLike)));

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
        [JsonIgnore]
        public abstract DataContextStackManipulationAttribute DataContextManipulationAttribute { get; }


        public ControlResolverMetadataBase(IControlType controlType)
        {
            this.controlType = controlType;
            this.attribute = controlType?.Type?.GetControlMarkupOptionsAttribute();

            this.properties = new Lazy<Dictionary<string, IPropertyDescriptor>>(() => {
                var result = new Dictionary<string, IPropertyDescriptor>(StringComparer.CurrentCultureIgnoreCase);
                LoadProperties(result);
                return result;
            });
            this._propertyGroups = new Lazy<List<PropertyGroupMatcher>>(() =>
            {
                var propertyGroups = new List<PropertyGroupMatcher>();
                LoadPropertyGroups(propertyGroups);
                return propertyGroups;
            });
        }

        protected abstract void LoadProperties(Dictionary<string, IPropertyDescriptor> result);

        protected abstract void LoadPropertyGroups(List<PropertyGroupMatcher> result);
    }
}
