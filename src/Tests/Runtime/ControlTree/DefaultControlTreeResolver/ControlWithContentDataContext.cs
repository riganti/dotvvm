using System;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver
{
    [DataContextChanger]
    public class ControlWithContentDataContext : DotvvmControl
    {
        public class DataContextChanger : DataContextChangeAttribute
        {
            public override int Order => 0;

            public override ITypeDescriptor GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor property = null)
            {
                return new ResolvedTypeDescriptor(typeof(int));
            }

            public override Type GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, DotvvmBindableObject control, DotvvmProperty property = null)
            {
                return typeof(int);
            }
        }
    }
}
