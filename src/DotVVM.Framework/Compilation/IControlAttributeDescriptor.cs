using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation
{
	public interface IControlAttributeDescriptor
	{
        string  Name { get; }

        MarkupOptionsAttribute MarkupOptions { get; }
        DataContextChangeAttribute[] DataContextChangeAttributes { get; }
		DataContextStackManipulationAttribute DataContextManipulationAttribute { get; }
        ITypeDescriptor DeclaringType { get; }
        ITypeDescriptor PropertyType { get; }
	}
}
