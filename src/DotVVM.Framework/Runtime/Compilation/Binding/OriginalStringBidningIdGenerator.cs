using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public class OriginalStringBidningIdGenerator : IBindingIdGenerator
    {
        public string GetId(ResolvedBinding binding, string fileHash)
        {
            return binding.Value;
        }
    }
}
