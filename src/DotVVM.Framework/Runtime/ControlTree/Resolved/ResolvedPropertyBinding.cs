using System.Diagnostics;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Runtime.ControlTree.Resolved
{
    [DebuggerDisplay("{Property}='{{Binding}}'")]
    public class ResolvedPropertyBinding : ResolvedPropertySetter, IAbstractPropertyBinding
    {
        public ResolvedBinding Binding { get; set; }

        public ResolvedPropertyBinding(DotvvmProperty property, ResolvedBinding binding)
            :base(property)
        {
            Binding = binding;
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitPropertyBinding(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
        }

        IAbstractBinding IAbstractPropertyBinding.Binding => Binding;
    }
}
