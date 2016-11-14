using System;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    [CommandBindingCompilation]
    public class ControlCommandBindingExpression : CommandBindingExpression
    {
        public ControlCommandBindingExpression() { }

        public ControlCommandBindingExpression(CompiledBindingExpression expression)
            : base(expression) { }
    }
}