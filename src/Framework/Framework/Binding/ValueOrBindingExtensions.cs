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
    public static object? UnwrapToObject(this ValueOrBinding vob) =>
        vob.BindingOrDefault ?? vob.BoxedValue;
    public static object? UnwrapToObject(object? obj) =>
        obj is ValueOrBinding vob ? vob.UnwrapToObject() : obj;
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
    public static IValueBinding<U> Select<T, U>(this IValueBinding<T> binding, Expression<Func<T, U>> mapping) =>
        (IValueBinding<U>)SelectImpl(binding, mapping);
    public static IStaticValueBinding<U> Select<T, U>(this IStaticValueBinding<T> binding, Expression<Func<T, U>> mapping) =>
        (IStaticValueBinding<U>)SelectImpl(binding, mapping);
    public static ValueOrBinding<U> Select<T, U>(this ValueOrBinding<T> vob, Expression<Func<T, U>> mapping) =>
        vob.HasBinding ? new(SelectImpl(vob.BindingOrDefault, mapping))
                       : new(mapping.Compile(preferInterpretation: true).Invoke(vob.ValueOrDefault));
}
