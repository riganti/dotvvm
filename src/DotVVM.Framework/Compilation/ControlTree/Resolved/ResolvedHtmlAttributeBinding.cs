using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    [DebuggerDisplay("{Name}='{{Binding}}'")]
    public class ResolvedHtmlAttributeBinding : ResolvedHtmlAttributeSetter, IAbstractHtmlAttributeBinding
    {
        IAbstractBinding IAbstractHtmlAttributeBinding.Binding => Binding;
        public ResolvedBinding Binding {get; set;}

        public ResolvedHtmlAttributeBinding(string name, ResolvedBinding binding)
            : base(name)
        {
            Binding = binding;
            binding.Parent = this;
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitHtmlAttributeBinding(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            Binding?.Accept(visitor);
        }
    }
}
