using System;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    [BindingCompilationRequirements(Javascript = BindingCompilationRequirementType.StronglyRequire)]
    [StaticCommandBindingCompilation]
    public class StaticCommandBindingExpression : CommandBindingExpression
    {
        public StaticCommandBindingExpression(CompiledBindingExpression expression)
            : base(expression)
        { }

        public override Delegate GetCommandDelegate(DotvvmBindableObject control, DotvvmProperty property)
        {
            throw new NotImplementedException();
        }
    }
}
