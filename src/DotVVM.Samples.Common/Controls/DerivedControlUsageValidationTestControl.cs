using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;

namespace DotVVM.Samples.Common.Controls
{
    public class DerivedControlUsageValidationTestControl : ControlUsageValidationTestControl
    {
        [ControlUsageValidator(Override = true)]
        public static new IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            yield break;
        }
    }
}
