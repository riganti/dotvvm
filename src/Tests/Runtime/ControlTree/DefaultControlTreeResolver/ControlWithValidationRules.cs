using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Compilation.Validation;

namespace DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver
{
    public class ControlWithValidationRules : HtmlGenericControl
    {
        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> Validate1(ResolvedControl control)
        {
            if (!control.Properties.ContainsKey(VisibleProperty))
                yield return new ControlUsageError($"The Visible property is required");
        }

        [ControlUsageValidator]
        public static IEnumerable<string> Validate2(DothtmlElementNode control)
        {
            if (control.Attributes.Count != 2)
                yield return $"The control has to have exactly two attributes";
        }
    }
}
