using System;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Compilation;

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
