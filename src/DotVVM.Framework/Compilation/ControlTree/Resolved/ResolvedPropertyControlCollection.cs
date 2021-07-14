using System.Collections.Generic;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedPropertyControlCollection : ResolvedPropertySetter, IAbstractPropertyControlCollection
    {
        public List<ResolvedControl> Controls { get; set; }

        IEnumerable<IAbstractControl> IAbstractPropertyControlCollection.Controls => Controls;

        public ResolvedPropertyControlCollection(DotvvmProperty property, List<ResolvedControl> controls) : base(property)
        {
            Controls = controls;

            controls.ForEach(c => c.Parent = this);
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

        public override string ToString() => $"{Property}={string.Join("", Controls)}";
    }
}
