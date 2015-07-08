using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    [DebuggerDisplay("{Type.Name}: {Value}")]
    public class ResolvedBinding
    {
        public Type Type { get; set; }
        public string Value { get; set; }
    }
}
