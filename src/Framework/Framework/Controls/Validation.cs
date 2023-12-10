using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Validation;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class Validation
    {
        [AttachedProperty(typeof(bool))]
        [MarkupOptions(AllowBinding = false)]
        public static DotvvmProperty EnabledProperty = DotvvmProperty.Register<bool, Validation>(() => EnabledProperty, true, true);

        [AttachedProperty(typeof(object))]
        [MarkupOptions(AllowHardCodedValue = false)]
        public static DotvvmProperty TargetProperty = DotvvmProperty.Register<object?, Validation>(() => TargetProperty, null, true);


        [ControlUsageValidator(IncludeAttachedProperties = true)]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            if (control.GetValue(Validation.TargetProperty) is ResolvedPropertyBinding { Binding: {} targetBinding })
            {
                if (targetBinding.ResultType.IsPrimitiveTypeDescriptor() &&
                    targetBinding.Expression is not ConstantExpression // Allow Target={value: 0}, it's an OK hack to supress automatic validation 
                )
                {
                    yield return new ControlUsageError(
                        $"Validation.Target should be bound to a complex object instead of '{targetBinding.ResultType!.CSharpName}'. Validation attributes on the specified property are ignored, only the rules inside the target object are validated.",
                        DiagnosticSeverity.Warning,
                        (targetBinding.DothtmlNode as DothtmlBindingNode)?.ValueNode ?? targetBinding.DothtmlNode
                    );
                }
            }
        }
    }
}
