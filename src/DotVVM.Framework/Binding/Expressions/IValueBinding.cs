#nullable enable
using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IValueBinding : IStaticValueBinding
    {
        ParametrizedCode KnockoutExpression { get; }
        ParametrizedCode UnwrappedKnockoutExpression { get; }
        ParametrizedCode WrappedKnockoutExpression { get; }
    }

    public interface IValueBinding<out T> : IValueBinding, IStaticValueBinding<T>
    {
    }
}
