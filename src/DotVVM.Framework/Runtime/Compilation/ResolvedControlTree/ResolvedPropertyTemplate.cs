using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public class ResolvedPropertyTemplate : ResolvedPropertySetter
    {
        public List<ResolvedControl> Content { get; set; }

        public ResolvedPropertyTemplate(DotvvmProperty property, List<ResolvedControl> content)
            : base(property)
        {
            Content = content;
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitPropertyTemplate(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            foreach (var c in Content)
            {
                c.Accept(visitor);
            }
        }
    }
}
