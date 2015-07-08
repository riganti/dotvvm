using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public abstract class ResolvedPropertySetter : IResolvedTreeNode
    {
        public DotvvmProperty Property { get; set; }

        public ResolvedPropertySetter(DotvvmProperty property)
        {
            Property = property;
        }

        public abstract void Accept(IResolvedControlTreeVisitor visitor);

        public abstract void AcceptChildren(IResolvedControlTreeVisitor visitor);
    }
}
