#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IPropertyGroupDescriptor: IControlAttributeDescriptor
    {
        string[] Prefixes { get; }
        ITypeDescriptor? CollectionType { get; }
        IPropertyDescriptor GetDotvvmProperty(string name);
    }
}
