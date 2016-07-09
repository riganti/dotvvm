using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree
{
	public class PopDataContextManipulationAttribute : DataContextStackManipulationAttribute
	{
		public override IDataContextStack ChangeStackForChildren(IDataContextStack original, IAbstractControl control, IPropertyDescriptor property, Func<IDataContextStack, ITypeDescriptor, IDataContextStack> createNewFrame)
		{
			return original.Parent;
		}
	}
}
