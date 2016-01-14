using DotVVM.Framework.Runtime.ControlTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.Validation
{
    public interface IControlUsageValidator
    {
        IEnumerable<ControlUsageError> Validate(IAbstractControl control);
    }
}
