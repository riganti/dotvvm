using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
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
