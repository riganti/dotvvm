using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public class ResolvedPropertyControlCollection : ResolvedPropertySetter, IAbstractPropertyControlCollection
    {
        public List<ResolvedControl> Controls { get; set; }

        IEnumerable<IAbstractControl> IAbstractPropertyControlCollection.Controls => Controls;

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
