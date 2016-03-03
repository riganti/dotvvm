using System;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
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