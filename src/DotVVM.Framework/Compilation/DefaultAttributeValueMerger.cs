using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation
{
	public class DefaultAttributeValueMerger : IAttributeValueMerger
	{
		public IAbstractPropertySetter MergeValues(IAbstractPropertySetter a, IAbstractPropertySetter b, out string error)
		{
			error = "Merge not supported";
			return null;
		}
	}
}
