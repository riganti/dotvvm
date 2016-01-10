using System;
using System.Collections.Generic;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
{
    public class DesignTimeControl : DesignTimeContentNode, IAbstractControl
    {

        private Dictionary<IPropertyDescriptor, DesignTimePropertySetter> properties = new Dictionary<IPropertyDescriptor, DesignTimePropertySetter>();
        private Dictionary<string, object> htmlAttributes = new Dictionary<string, object>();

        public DesignTimeControl(DothtmlNode node, IControlResolverMetadata metadata) : base(node, metadata)
        {
        }

        public IEnumerable<IPropertyDescriptor> PropertyNames => properties.Keys;

        public IReadOnlyDictionary<string, object> HtmlAttributes => htmlAttributes;

        public object[] ConstructorParameters { get; set; }

        public bool TryGetProperty(IPropertyDescriptor property, out IAbstractPropertySetter value)
        {
            DesignTimePropertySetter setter;
            var result = properties.TryGetValue(property, out setter);
            value = setter;
            return result;
        }



        public void SetBinding(DesignTimePropertyBinding binding)
        {
            properties[binding.Property] = binding;
        }

        public void SetValue(DesignTimePropertyValue value)
        {
            properties[value.Property] = value;
        }

        public void SetHtmlAttribute(string attributeName, object value)
        {
            htmlAttributes[attributeName] = value;
        }

        public void SetProperty(DesignTimePropertySetter setter)
        {
            properties[setter.Property] = setter;
        }

    }
}