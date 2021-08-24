#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using Newtonsoft.Json;

public static class ValueOrBindingExtensions
{
    public static ValueOrBinding<bool> Negate(this ValueOrBinding<bool> v)
    {
        if (v.BindingOrDefault is IBinding binding)
            return new ValueOrBinding<bool>(
                binding.GetProperty<NegatedBindingExpression>().Binding
            );
        else
            return !v.ValueOrDefault;
    }
    public static ValueOrBinding<bool?> Negate(this ValueOrBinding<bool?> v)
    {
        if (v.BindingOrDefault is IBinding binding)
            return new ValueOrBinding<bool?>(
                binding.GetProperty<NegatedBindingExpression>().Binding
            );
        else
            return !v.ValueOrDefault;
    }
    public static T Negate<T>(this T binding)
        where T: IStaticValueBinding<bool>
    {
        return (T)binding.GetProperty<NegatedBindingExpression>().Binding;
    }
    public static ValueOrBinding<bool> IsMoreThanZero(this ValueOrBinding<int> v)
    {
        if (v.BindingOrDefault is IBinding binding)
            return new ValueOrBinding<bool>(
                binding.GetProperty<IsMoreThanZeroBindingProperty>().Binding
            );
        else
            return v.ValueOrDefault > 0;
    }
    public static ValueOrBinding<IList<T>> GetItems<T>(this ValueOrBinding<IBaseGridViewDataSet<T>> v)
    {
        if (v.BindingOrDefault is IBinding binding)
            return new ValueOrBinding<IList<T>>(
                binding.GetProperty<DataSourceAccessBinding>().Binding
            );
        else
            return new ValueOrBinding<IList<T>>(v.ValueOrDefault.Items);
    }
    public static ValueOrBinding<string> AsString<T>(this ValueOrBinding<T> v)
    {
        if (v.BindingOrDefault is IBinding binding)
            return new ValueOrBinding<string>(
                binding.GetProperty<ExpectedAsStringBindingExpression>().Binding
            );
        else
            return new ValueOrBinding<string>("" + v.ValueOrDefault);
    }
}
