using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Parser;
using System.Text.RegularExpressions;

namespace DotVVM.Framework.Binding
{
    [BindingCompilationRequirements(Delegate = BindingCompilationRequirementType.StronglyRequire,
        Javascript = BindingCompilationRequirementType.IfPossible)]
    public class ControlPropertyBindingExpression : BindingExpression
    {
        public ControlPropertyBindingExpression()
        {
        }

        public ControlPropertyBindingExpression(CompiledBindingExpression expression)
            : base(expression)
        {
        }



        public override object Evaluate(DotvvmBindableControl control, DotvvmProperty property)
        {
            // I can execute the delegate, or find the property and get its value
            // TODO: whats better ??
            return ExecDelegate(control, true);


            //// find the parent markup control and calculate number of DataContext changes
            //int numberOfDataContextChanges;
            //var current = control.GetClosestControlBindingTarget(out numberOfDataContextChanges);

            //// get the property
            //var sourceProperty = DotvvmProperty.ResolveProperty(current.GetType(), OriginalString);
            //if (sourceProperty == null)
            //{
            //    throw new Exception(string.Format("The markup control of type '{0}' does not have a property '{1}'!", current.GetType(), ExpressionTree));        // TODO: exception handling
            //}

            //// check whether the property contains binding
            //if (current is DotvvmBindableControl)
            //{
            //    var originalBinding = ((DotvvmBindableControl)current).GetBinding(sourceProperty);
            //    if (originalBinding != null && originalBinding.GetType() == typeof(ValueBindingExpression))
            //    {
            //        // ValueBindingExpression must be modified to be evaluated against the original DataContext
            //        return new ValueBindingExpression(string.Join(".",
            //            Enumerable.Repeat("$parent", numberOfDataContextChanges)
            //            .Concat(new[] { originalBinding.OriginalString })));
            //    }
            //    else if (originalBinding != null)
            //    {
            //        return originalBinding;
            //    }
            //}

            //// otherwise evaluate on server
            //return current.GetValue(sourceProperty);
        }
        public override string TranslateToClientScript(DotvvmBindableControl control, DotvvmProperty property)
        {
            return Javascript;
        }
    }
}
