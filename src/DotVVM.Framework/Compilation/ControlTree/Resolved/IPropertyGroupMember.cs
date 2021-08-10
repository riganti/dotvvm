using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
	public interface IPropertyGroupMember
	{
		string Name { get; }
		IPropertyGroupDescriptor PropertyGroup { get; }
	}
}
