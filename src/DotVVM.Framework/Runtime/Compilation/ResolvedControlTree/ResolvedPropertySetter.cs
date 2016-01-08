using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public abstract class ResolvedPropertySetter : IResolvedTreeNode, IAbstractPropertySetter
    {
        public DotvvmProperty Property { get; set; }

        IPropertyDescriptor IAbstractPropertySetter.Property => Property;

        public ResolvedPropertySetter(DotvvmProperty property)
        {
            Property = property;
        }

        public abstract void Accept(IResolvedControlTreeVisitor visitor);

        public abstract void AcceptChildren(IResolvedControlTreeVisitor visitor);
    }
}
