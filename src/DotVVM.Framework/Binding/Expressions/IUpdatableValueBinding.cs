using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IUpdatableValueBinding: IBinding
    {
        CompiledBindingExpression.BindingUpdateDelegate UpdateDelegate { get; }
    }

    public interface IUpdatableValueBinding<in T>: IBinding
    {
        CompiledBindingExpression.BindingUpdateDelegate<T> UpdateDelegate { get; }
    }
}