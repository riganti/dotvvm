using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public class ControlCommandBindingExpression : CommandBindingExpression
    {

        
        public ControlCommandBindingExpression()
        {
        }

        public ControlCommandBindingExpression(CompiledBindingExpression expression)
            : base(expression)
        {
        }

        public override object Evaluate(DotvvmBindableControl control, DotvvmProperty property)
        {
            return ExecDelegate(control, true, true);
        }

    }
}