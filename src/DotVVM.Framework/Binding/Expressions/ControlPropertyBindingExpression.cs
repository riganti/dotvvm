using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
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
            return ExecDelegate(control, property != DotvvmBindableObject.DataContextProperty);
        }
    }
}
