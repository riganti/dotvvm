using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Runtime.ControlTree.Resolved
{
    public class ResolvedControl : ResolvedContentNode, IAbstractControl
    {
        public Dictionary<DotvvmProperty, ResolvedPropertySetter> Properties { get; set; } = new Dictionary<DotvvmProperty, ResolvedPropertySetter>();
        
        public Dictionary<string, object> HtmlAttributes { get; set; }

        public object[] ConstructorParameters { get; set; }

        IEnumerable<IPropertyDescriptor> IAbstractControl.PropertyNames => Properties.Keys;

        IReadOnlyDictionary<string, object> IAbstractControl.HtmlAttributes => HtmlAttributes;

        public ResolvedControl(ControlResolverMetadata metadata, DothtmlNode node, DataContextStack dataContext)
            : base(metadata, node, dataContext)
        {
        }

        public void SetProperty(ResolvedPropertySetter value)
        {
            Properties[value.Property] = value;
            value.Parent = this;
        }
        
        public void SetHtmlAttribute(string attributeName, object value)
        {
            if (HtmlAttributes == null) HtmlAttributes = new Dictionary<string, object>();
            object currentValue;
            if (HtmlAttributes.TryGetValue(attributeName, out currentValue))
            {
                if (!(value is string && currentValue is string)) throw new NotSupportedException("multiple binding values are not supported in one attribute");
                HtmlAttributes[attributeName] = Controls.HtmlWriter.JoinAttributeValues(attributeName, (string)currentValue, (string)value);
            }
            else HtmlAttributes[attributeName] = value;

            if (value is ResolvedBinding)
            {
                ((ResolvedBinding) value).Parent = this;
            }
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitControl(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            foreach (var prop in Properties.Values)
            {
                prop.Accept(visitor);
            }
            base.AcceptChildren(visitor);
        }


        public bool TryGetProperty(IPropertyDescriptor property, out IAbstractPropertySetter value)
        {
            ResolvedPropertySetter result;
            value = null;
            if (!Properties.TryGetValue((DotvvmProperty)property, out result)) return false;
            value = result;
            return true;
        }
    }
}
