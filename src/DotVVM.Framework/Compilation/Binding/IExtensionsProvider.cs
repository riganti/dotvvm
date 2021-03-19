using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Binding
{
    public interface IExtensionsProvider
    {
        IEnumerable<MethodInfo> GetExtensionMethods();
    }
}
