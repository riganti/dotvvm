using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public class ResolvedPropertyControlCollection : ResolvedPropertySetter
    {
        public List<ResolvedControl> Controls { get; set; }

        public ResolvedPropertyControlCollection(DotvvmProperty property, List<ResolvedControl> controls)
            : base(property)
        {
            Controls = controls;
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitPropertyControlCollection(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            foreach (var c in Controls)
            {
                c.Accept(visitor);
            }
        }
    }
}
