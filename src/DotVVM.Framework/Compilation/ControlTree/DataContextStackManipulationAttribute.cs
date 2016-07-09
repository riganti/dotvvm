using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree
{
	public abstract class DataContextStackManipulationAttribute: Attribute
	{
		public abstract IDataContextStack ChangeStackForChildren(IDataContextStack original, IAbstractControl control, IPropertyDescriptor property, Func<IDataContextStack, ITypeDescriptor, IDataContextStack> createNewFrame);
	}
}
