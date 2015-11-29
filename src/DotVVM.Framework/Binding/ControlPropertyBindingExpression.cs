using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Parser;
using System.Text.RegularExpressions;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Binding
{
    [BindingCompilationRequirements(Delegate = BindingCompilationRequirementType.StronglyRequire,
        Javascript = BindingCompilationRequirementType.IfPossible)]
    [BindingCompilation]
    public class ControlPropertyBindingExpression : ValueBindingExpression
    {
        public ControlPropertyBindingExpression() { }

        public ControlPropertyBindingExpression(CompiledBindingExpression expression)
            : base(expression)
        {
        }



        public override object Evaluate(DotvvmBindableObject control, DotvvmProperty property)
        {
            return ExecDelegate(control, property != DotvvmBindableObject.DataContextProperty, setRootControl: true);
        }
    }
}
