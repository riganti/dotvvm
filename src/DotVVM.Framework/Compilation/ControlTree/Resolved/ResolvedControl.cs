using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedControl : ResolvedContentNode, IAbstractControl
    {
        public Dictionary<DotvvmProperty, ResolvedPropertySetter> Properties { get; set; } = new Dictionary<DotvvmProperty, ResolvedPropertySetter>();
        public Dictionary<string, ResolvedHtmlAttributeSetter> HtmlAttributes { get; set; } = new Dictionary<string, ResolvedHtmlAttributeSetter>();

        public object[] ConstructorParameters { get; set; }

        IEnumerable<IPropertyDescriptor> IAbstractControl.PropertyNames => Properties.Keys;

        IEnumerable<IAbstractHtmlAttributeSetter> IAbstractControl.HtmlAttributes => HtmlAttributes.Values;

        public ResolvedControl(ControlResolverMetadata metadata, DothtmlNode node, DataContextStack dataContext)
            : base(metadata, node, dataContext)
        {
        }

        public void SetProperty(ResolvedPropertySetter value)
        {
            Properties[value.Property] = value;
            value.Parent = this;
        }
        
        public void SetHtmlAttribute(ResolvedHtmlAttributeSetter value)
        {
            HtmlAttributes[value.Name] = value;
            value.Parent = this;
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
            foreach (var att in HtmlAttributes.Values)
            {
                att.Accept(visitor);
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
