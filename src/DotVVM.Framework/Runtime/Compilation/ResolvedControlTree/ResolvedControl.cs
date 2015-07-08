using DotVVM.Framework.Binding;
using DotVVM.Framework.Parser.Dothtml.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public class ResolvedControl: ResolvedContentNode
    {
        public Dictionary<DotvvmProperty, ResolvedPropertySetter> Properties { get; set; } = new Dictionary<DotvvmProperty, ResolvedPropertySetter>();
        public Dictionary<string, object> HtmlAttributes { get; set; }
        public object[] ContructorParameters { get; set; }
        public ResolvedControl(ControlResolverMetadata metadata, DothtmlNode node)
            : base(metadata, node)
        {
        }

        public void SetProperty(ResolvedPropertySetter value)
        {
            Properties[value.Property] = value;
        }

        public void SetPropertyValue(DotvvmProperty property, object value)
        {
            Properties[property] = new ResolvedPropertyValue(property, value);
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
    }
}
