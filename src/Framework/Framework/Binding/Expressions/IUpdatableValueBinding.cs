using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IUpdatableValueBinding: IBinding
    {
        BindingUpdateDelegate UpdateDelegate { get; }
    }

    public interface IUpdatableValueBinding<in T>: IBinding
    {
        BindingUpdateDelegate<T> UpdateDelegate { get; }
    }
}
