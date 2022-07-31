using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;

namespace DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver
{
    public class ControlWithOverriddenRules : ControlWithValidationRules
    {
        [ControlUsageValidator(Override = true)]
        public static IEnumerable<ControlUsageError> Validate(ResolvedControl control)
        {
            yield break;
        }
    }
}

