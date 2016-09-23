using DotVVM.Framework.Compilation.ControlTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation
{
	public interface IAttributeValueMerger
	{
		IAbstractPropertySetter MergeValues(IAbstractPropertySetter a, IAbstractPropertySetter b, out string error);
	}
}
