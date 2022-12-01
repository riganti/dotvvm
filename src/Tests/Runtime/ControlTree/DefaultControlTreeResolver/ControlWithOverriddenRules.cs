using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Controls;

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
    
    [ControlMarkupOptions(PrimaryName = "PrimaryNameControl")]
    public class ControlWithPrimaryName : DotvvmControl
    {
    }

    [ControlMarkupOptions(AlternativeNames = new [] { "AlternativeNameControl", "AlternativeNameControl2" })]
    public class ControlWithAlternativeNames : DotvvmControl
    {
    }
}

