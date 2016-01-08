using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Styles
{
    public interface IStyleApplicator
    {
        void ApplyStyle(ResolvedControl control);
    }
}
