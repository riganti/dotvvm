using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;

public static class ValueOrBindingExtensions
{
    /// <summary> Returns the value or the binding from the ValueOrBinding container. Equivalent to calling <code>vob.BindingOrDefault ?? vob.BoxedValue</code> </summary>
    public static object? UnwrapToObject(this ValueOrBinding vob) =>
        vob.BindingOrDefault ?? vob.BoxedValue;

    /// <summary> If the obj is ValueOrBinding, returns the binding or the value from the container. Equivalent to <code>obj is ValueOrBinding vob ? vob.UnwrapToObject() : obj</code> </summary>
    public static object? UnwrapToObject(object? obj) =>
        obj is ValueOrBinding vob ? vob.UnwrapToObject() : obj;

    /// <summary> Returns ValueOrBinding with the value of `!a`. The resulting binding is cached, so it's safe to use this method at runtime. </summary>
    public static ValueOrBinding<bool> Negate(this ValueOrBinding<bool> v)
    {
        if (v.BindingOrDefault is IBinding binding)
            return new ValueOrBinding<bool>(
                binding.GetProperty<NegatedBindingExpression>().Binding
            );
        else
            return !v.ValueOrDefault;
    }
    /// <summary> Returns ValueOrBinding with the value of `!a`. The resulting binding is cached, so it's safe to use this method at runtime. </summary>
    public static ValueOrBinding<bool?> Negate(this ValueOrBinding<bool?> v)
    {
        if (v.BindingOrDefault is IBinding binding)
            return new ValueOrBinding<bool?>(
                binding.GetProperty<NegatedBindingExpression>().Binding
            );
        else
            return !v.ValueOrDefault;
    }
    /// <summary> Returns a binding with the value of `!bindingValue`. The resulting binding is cached, so it's safe to use this method at runtime. </summary>
    public static T Negate<T>(this T binding)
        where T: IStaticValueBinding<bool>
    {
        return (T)binding.GetProperty<NegatedBindingExpression>().Binding;
    }
    /// <summary> Returns ValueOrBinding with the value of `a > 0`. The resulting binding is cached, so it's safe to use this method at runtime. </summary>
    public static ValueOrBinding<bool> IsMoreThanZero(this ValueOrBinding<int> v)
    {
        if (v.BindingOrDefault is IBinding binding)
            return new ValueOrBinding<bool>(
                binding.GetProperty<IsMoreThanZeroBindingProperty>().Binding
            );
        else
            return v.ValueOrDefault > 0;
    }
    /// <summary> Returns ValueOrBinding with the value of `a.Items` where a is grid view dataset. The resulting binding is cached, so it's safe to use this method at runtime. </summary>
    public static ValueOrBinding<IList<T>> GetItems<T>(this ValueOrBinding<IBaseGridViewDataSet<T>> v)
    {
        if (v.BindingOrDefault is IBinding binding)
            return new ValueOrBinding<IList<T>>(
                binding.GetProperty<DataSourceAccessBinding>().Binding
            );
        else
            return new ValueOrBinding<IList<T>>(v.ValueOrDefault.Items);
    }
    /// <summary> Returns ValueOrBinding with the value of `a?.ToString() ?? ""`. The resulting binding is cached, so it's safe to use this method at runtime. </summary>
    public static ValueOrBinding<string> AsString<T>(this ValueOrBinding<T> v)
    {
        if (v.BindingOrDefault is IBinding binding)
            return new ValueOrBinding<string>(
                binding.GetProperty<ExpectedAsStringBindingExpression>().Binding
            );
        else
            return new ValueOrBinding<string>("" + v.ValueOrDefault);
    }
    /// <summary> Returns ValueOrBinding with the value of `a is object`. The resulting binding is cached, so it's safe to use this method at runtime. </summary>
    public static ValueOrBinding<bool> NotNull<T>(this ValueOrBinding<T> v) =>
        v.IsNull().Negate();
    /// <summary> Returns ValueOrBinding with the value of `a is null`. The resulting binding is cached, so it should be safe to use this method at runtime. </summary>
    public static ValueOrBinding<bool> IsNull<T>(this ValueOrBinding<T> v)
    {
        if (v.HasBinding)
            return new(v.BindingOrDefault.GetProperty<IsNullBindingExpression>().Binding);
        else
            return new(v.ValueOrDefault is null);
    }
    /// <summary> Returns ValueOrBinding with the value of `!string.IsNullOrEmpty(a)`. The resulting binding is cached, so it should be safe to use this method at runtime. </summary>
    public static ValueOrBinding<bool> NotNullOrEmpty(this ValueOrBinding<string> v) =>
        v.IsNullOrEmpty().Negate();
    /// <summary> Returns ValueOrBinding with the value of `string.IsNullOrEmpty(a)`. The resulting binding is cached, so it's safe to use this method at runtime. </summary>
    public static ValueOrBinding<bool> IsNullOrEmpty(this ValueOrBinding<string> v)
    {
        if (v.HasBinding)
            return new(v.BindingOrDefault.GetProperty<IsNullOrEmptyBindingExpression>().Binding);
        else
            return new(string.IsNullOrEmpty(v.ValueOrDefault));
    }
    /// <summary> Returns ValueOrBinding with the value of `!string.IsNullOrWhitespace(a)`. The resulting binding is cached, so it's safe to use this method at runtime. </summary>
    public static ValueOrBinding<bool> NotNullOrWhitespace(this ValueOrBinding<string> v) =>
        v.IsNullOrWhitespace().Negate();

    /// <summary> Returns ValueOrBinding with the value of `string.IsNullOrWhitespace(a)`. The resulting binding is cached, so it's safe to use this method at runtime. </summary>
    public static ValueOrBinding<bool> IsNullOrWhitespace(this ValueOrBinding<string> v)
    {
        if (v.HasBinding)
            return new(v.BindingOrDefault.GetProperty<IsNullOrWhitespaceBindingExpression>().Binding);
        else
            return new(string.IsNullOrWhiteSpace(v.ValueOrDefault));
    }

    /// <summary> Returns ValueOrBinding with the value of `a &amp;&amp; b`. If both a and b contain a binding, they are combined together. The result is cached, so it's safe to use this method at runtime. </summary>
    public static ValueOrBinding<bool> And(this ValueOrBinding<bool> a, ValueOrBinding<bool> b)
    {
        if (a.HasValue)
            return a.ValueOrDefault ? b : false;
        if (b.HasValue)
            return b.ValueOrDefault ? a : false;
        return new(BindingCombinator.GetCombination(
            BindingCombinator.AndAlsoCombination,
            a.BindingOrDefault,
            b.BindingOrDefault));
    }

    /// <summary> Returns ValueOrBinding with the value of `a || b`. If both a and b contain a binding, they are combined together. The result is cached, so it's safe to use this method at runtime. </summary>
    public static ValueOrBinding<bool> Or(this ValueOrBinding<bool> a, ValueOrBinding<bool> b)
    {
        if (a.HasValue)
            return a.ValueOrDefault ? true : b;
        if (b.HasValue)
            return b.ValueOrDefault ? true : a;
        return new(BindingCombinator.GetCombination(
            BindingCombinator.OrElseCombination,
            a.BindingOrDefault,
            b.BindingOrDefault));
    }

    internal static IBinding CreateConstantBinding(
        object constant,
        Type type,
        BindingCompilationService service,
        BindingParserOptions bpo) =>
        service.Cache.CreateCachedBinding(
            "dotvvm-ConstantBinding",
            new object [] { type, constant, bpo },
            () => {
                var expr = Expression.Constant(constant, type);
                return service.CreateBinding(bpo.BindingType, new object[] {
                    bpo,
                    new ExpectedTypeBindingProperty(type),
                    new ResultTypeBindingProperty(type),
                    new ParsedExpressionBindingProperty(expr),
                    new CastedExpressionBindingProperty(expr),
                    new KnockoutJsExpressionBindingProperty(JavascriptTranslationVisitor.TranslateConstant(expr)),
                    new BindingDelegate((_, _) => constant)
                });
            });

    internal static IBinding SelectImpl(IBinding binding, LambdaExpression mapping)
    {
        var service = binding.GetProperty<BindingCompilationService>();
        var parserOptions = binding.GetProperty<BindingParserOptions>();
        // get reasonable hash key by captured variables into constants
        var optimizedExpr = (LambdaExpression)mapping.OptimizeConstants();

        if (optimizedExpr.Body is ConstantExpression constantBody)
            return CreateConstantBinding(constantBody.Value, mapping.ReturnType, service, parserOptions);

        return service.Cache.CreateCachedBinding(
                "Dotvvm-BindingSelect",
                new object[] {
                    new Tuple<IBinding>(binding),
                    new ObjectWithComparer<Expression>(optimizedExpr, ExpressionComparer.Instance)
                },
                () => binding.DeriveBinding(new object?[] {
                    new ExpectedTypeBindingProperty(mapping.ReturnType),
                    new ResultTypeBindingProperty(mapping.ReturnType),
                    new ParsedExpressionBindingProperty(
                        ExpressionUtils.Replace(
                            optimizedExpr,
                            Expression.Convert(
                                binding.GetProperty<ParsedExpressionBindingProperty>().Expression,
                                optimizedExpr.Parameters[0].Type
                            )
                        )
                    )
                })
            );
    }

    /// <summary> Maps the result of this binding with another expression. The expression is also translated to Javascript, so only translatable methods may be used.
    /// Note that this method is very fast, so use it carefully. Usage is server-side styles should be preferred over usage at runtime in custom controls. </summary>
    public static IValueBinding<U> Select<T, U>(this IValueBinding<T> binding, Expression<Func<T, U>> mapping) =>
        (IValueBinding<U>)SelectImpl(binding, mapping);
    /// <summary> Maps the result of this binding with another expression. The expression is also translated to Javascript, so only translatable methods may be used.
    /// Note that this method is very fast, so use it carefully. Usage is server-side styles should be preferred over usage at runtime in custom controls. </summary>
    public static IStaticValueBinding<U> Select<T, U>(this IStaticValueBinding<T> binding, Expression<Func<T, U>> mapping) =>
        (IStaticValueBinding<U>)SelectImpl(binding, mapping);
    /// <summary> Maps the result of this binding with another expression. The expression is also translated to Javascript, so only translatable methods may be used.
    /// Note that this method is very fast, so use it carefully. Usage is server-side styles should be preferred over usage at runtime in custom controls. </summary>
    public static ValueOrBinding<U> Select<T, U>(this ValueOrBinding<T> vob, Expression<Func<T, U>> mapping) =>
        vob.HasBinding ? new(SelectImpl(vob.BindingOrDefault, mapping))
                       : new(mapping.Compile(preferInterpretation: true).Invoke(vob.ValueOrDefault));
}
