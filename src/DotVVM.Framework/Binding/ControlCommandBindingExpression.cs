using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Binding
{
    [CommandBindingCompilation]
    public class ControlCommandBindingExpression : CommandBindingExpression
    {
        public ControlCommandBindingExpression()
        {
        }

        public ControlCommandBindingExpression(CompiledBindingExpression expression)
            : base(expression)
        {
        }

        public override Delegate GetCommandDelegate(DotvvmBindableObject control, DotvvmProperty property)
        {
            return (Delegate)ExecDelegate(control, true, true);
        }

    }
}