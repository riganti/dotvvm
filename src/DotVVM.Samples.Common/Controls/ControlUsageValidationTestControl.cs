using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Controls
{
    public class ControlUsageValidationTestControl : HtmlGenericControl
    {

        public ControlUsageValidationTestControl()
            :base("div")
        {

        }
        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            yield return new ControlUsageError("Error", control.DothtmlNode);
        }
    }
}
