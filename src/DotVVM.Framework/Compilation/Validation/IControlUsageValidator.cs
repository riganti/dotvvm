using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation.Validation
{
    public interface IControlUsageValidator
    {
        IEnumerable<ControlUsageError> Validate(IAbstractControl control);
    }
}
