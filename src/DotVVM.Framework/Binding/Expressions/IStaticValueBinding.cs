using System;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IStaticValueBinding: IBinding
    {
        CompiledBindingExpression.BindingDelegate BindingDelegate { get; }
        Type ResultType { get; }
    }

    public interface IStaticValueBinding<out T>: IStaticValueBinding
    {
        CompiledBindingExpression.BindingDelegate<T> BindingDelegate { get; }
    }
}
