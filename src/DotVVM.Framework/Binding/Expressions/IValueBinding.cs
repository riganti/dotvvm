using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IValueBinding : IStaticValueBinding
    {
        ParametrizedCode KnockoutExpression { get; }
        //string GetKnockoutBindingExpression();
    }

}
