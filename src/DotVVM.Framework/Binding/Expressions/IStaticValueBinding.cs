#nullable enable
using System;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IStaticValueBinding: IBinding
    {
        BindingDelegate BindingDelegate { get; }
        Type ResultType { get; }
    }

    public interface IStaticValueBinding<out T>: IStaticValueBinding
    {
        new BindingDelegate<T> BindingDelegate { get; }
    }
}
