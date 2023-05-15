using System.Collections.Generic;
using DotVVM.Framework.Binding;
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

    public class ControlWithExtractGenericArgument : DotvvmControl
    {

        [ExtractGenericArgumentDataContextChange(typeof(IEnumerable<>), 0)]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DotvvmProperty TextProperty
            = DotvvmProperty.Register<string, ControlWithExtractGenericArgument>(c => c.Text, null);
    }
}

