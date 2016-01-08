using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public interface IBindingIdGenerator
    {
        string GetId(ResolvedBinding binding, string fileHash);
    }
}
