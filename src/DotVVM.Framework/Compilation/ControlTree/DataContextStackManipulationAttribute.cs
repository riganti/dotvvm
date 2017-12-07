using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public abstract class DataContextStackManipulationAttribute: Attribute
    {
        public abstract IDataContextStack ChangeStackForChildren(IDataContextStack original, IAbstractControl control, IPropertyDescriptor property, Func<IDataContextStack, ITypeDescriptor, IDataContextStack> createNewFrame);

        public abstract DataContextStack ChangeStackForChildren(DataContextStack original, DotvvmBindableObject obj, DotvvmProperty property, Func<DataContextStack, Type, DataContextStack> createNewFrame);
    }
}
