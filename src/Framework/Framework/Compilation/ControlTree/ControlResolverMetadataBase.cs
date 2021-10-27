using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using Newtonsoft.Json;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public abstract class ControlResolverMetadataBase : IControlResolverMetadata
    {
        private readonly IControlType controlType;
        private readonly ControlMarkupOptionsAttribute? attribute;

        private readonly Lazy<Dictionary<string, IPropertyDescriptor>> properties;
        public IReadOnlyDictionary<string, IPropertyDescriptor> Properties => properties.Value;

        private readonly Lazy<List<PropertyGroupMatcher>> _propertyGroups;
        public IReadOnlyList<PropertyGroupMatcher> PropertyGroups => _propertyGroups.Value;


        public string? Namespace => controlType.Type.Namespace;

        public string Name => controlType.Type.Name;

        [JsonIgnore]
        public ITypeDescriptor Type => controlType.Type;

        public bool HasHtmlAttributesCollection => Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof (IControlWithHtmlAttributes)));

        [JsonIgnore]
        public IEnumerable<string> PropertyNames => Properties.Keys;

        public bool TryGetProperty(string name, [NotNullWhen(true)] out IPropertyDescriptor? value)
        {
            return Properties.TryGetValue(name, out value);
        }

        public bool IsContentAllowed =>
            (attribute?.AllowContent ?? true) &&
            Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof(IDotvvmControl))) &&
            // composite controls can not contain children, only content properties
            !Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof(CompositeControl)));

        [JsonIgnore]
        public IPropertyDescriptor? DefaultContentProperty
        {
            get
            {
                IPropertyDescriptor result;
                if (Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof(CompositeControl))))
                {
                    // properties Content and ContentTemplate are used as content by default, if they exist
                    if (Properties.TryGetValue("Content", out result))
                        return result;
                    if (Properties.TryGetValue("ContentTemplate", out result))
                        return result;
                }

                var prop = attribute?.DefaultContentProperty;

                if (string.IsNullOrEmpty(prop))
                {
                    return null;
                }

                return Properties.TryGetValue(prop, out result) ? result : null;
            }
        }

        public string? DefaultContentPropertyName => attribute?.DefaultContentProperty;

        [JsonIgnore]
        public string? VirtualPath => controlType?.VirtualPath;

        [JsonIgnore]
        public ITypeDescriptor? DataContextConstraint => controlType?.DataContextRequirement;

        [JsonIgnore]
        public IEnumerable<IPropertyDescriptor> AllProperties => Properties.Values;

        [JsonIgnore]
        public abstract DataContextChangeAttribute[] DataContextChangeAttributes { get; }
        [JsonIgnore]
        public abstract DataContextStackManipulationAttribute? DataContextManipulationAttribute { get; }


        public ControlResolverMetadataBase(IControlType controlType)
        {
            this.controlType = controlType;
            this.attribute = controlType?.Type?.GetControlMarkupOptionsAttribute();

            this.properties = new Lazy<Dictionary<string, IPropertyDescriptor>>(() => {
                var result = new Dictionary<string, IPropertyDescriptor>(StringComparer.OrdinalIgnoreCase);
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
