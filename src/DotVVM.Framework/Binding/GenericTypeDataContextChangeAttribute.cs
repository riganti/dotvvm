using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public class GenericTypeDataContextChangeAttribute : DataContextChangeAttribute
    {
        public Type GenericTypeDefinition { get; set; }

        public override int Order { get; }

        public GenericTypeDataContextChangeAttribute(Type genericTypeDefinition, int order = 0)
        {
            if (!genericTypeDefinition.IsGenericTypeDefinition || genericTypeDefinition.GetGenericArguments().Length != 1)
            {
                throw new ArgumentException("The type must be a generic type definition with one argument.", nameof(genericTypeDefinition));
            }

            GenericTypeDefinition = genericTypeDefinition;
            Order = order;
        }

        public override ITypeDescriptor GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor property = null)
        {
            return new ResolvedTypeDescriptor(GenericTypeDefinition).MakeGenericType(dataContext);
        }

        public override Type GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, DotvvmBindableObject control, DotvvmProperty property = null)
        {
            return GenericTypeDefinition.MakeGenericType(dataContext);
        }
    }
}
