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
    [CompileJavascript]
    public class ControlPropertyBindingExpression : ValueBindingExpression
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
            return ExecDelegate(control, property != DotvvmBindableControl.DataContextProperty, setRootControl: true);
        }
    }
}
