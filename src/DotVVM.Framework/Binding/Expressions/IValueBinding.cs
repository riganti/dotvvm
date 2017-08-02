using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IValueBinding : IStaticValueBinding
    {
        ParametrizedCode KnockoutExpression { get; }
        ParametrizedCode UnwrapedKnockoutExpression { get; }
        //string GetKnockoutBindingExpression();
    }

    public interface IValueBinding<out T> : IValueBinding, IStaticValueBinding<T>
    {
    }
}
