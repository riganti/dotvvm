using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Query;
public static partial class DotvvmFrameworkQueryExtensions
{
    public static IScalarQuery<U> ApplyOperation<T, U>(this IQuery<T> q, Expression<Func<IQueryable<T>, U>> f) =>
        ScalarQueryHelper.Make<T, U>(q, f);
    public static IScalarQuery<U> ApplyOperation<T, U>(this IScalarQuery<T> q, Expression<Func<T, U>> f) =>
        ScalarQueryHelper.Make<T, U>(q, f);

    public static IScalarQuery<TSource> First<TSource>(this IQuery<TSource> source) =>
        source.ApplyOperation(x => x.First());
    public static IScalarQuery<TSource> First<TSource>(this IQuery<TSource> source, Expression<Func<TSource, bool>> predicate) =>
        source.ApplyOperation(x => x.First(predicate));

    public static IScalarQuery<TSource> FirstOrDefault<TSource>(this IQuery<TSource> source) =>
        source.ApplyOperation(x => x.FirstOrDefault());

    public static IScalarQuery<TSource> FirstOrDefault<TSource>(this IQuery<TSource> source, Expression<Func<TSource, bool>> predicate) =>
        source.ApplyOperation(x => x.FirstOrDefault(predicate));

    public static IScalarQuery<TSource> Last<TSource>(this IQuery<TSource> source) =>
        source.ApplyOperation(x => x.Last());

    public static IScalarQuery<TSource> Last<TSource>(this IQuery<TSource> source, Expression<Func<TSource, bool>> predicate) =>
        source.ApplyOperation(x => x.Last(predicate));

    public static IScalarQuery<TSource> LastOrDefault<TSource>(this IQuery<TSource> source) =>
        source.ApplyOperation(x => x.LastOrDefault());

    public static IScalarQuery<TSource> LastOrDefault<TSource>(this IQuery<TSource> source, Expression<Func<TSource, bool>> predicate) =>
        source.ApplyOperation(x => x.LastOrDefault(predicate));

    public static IScalarQuery<TSource> Single<TSource>(this IQuery<TSource> source) =>
        source.ApplyOperation(x => x.Single());

    public static IScalarQuery<TSource> Single<TSource>(this IQuery<TSource> source, Expression<Func<TSource, bool>> predicate) =>
        source.ApplyOperation(x => x.Single(predicate));

    public static IScalarQuery<TSource> SingleOrDefault<TSource>(this IQuery<TSource> source) =>
        source.ApplyOperation(x => x.SingleOrDefault());

    public static IScalarQuery<TSource> SingleOrDefault<TSource>(this IQuery<TSource> source, Expression<Func<TSource, bool>> predicate) =>
        source.ApplyOperation(x => x.SingleOrDefault(predicate));

    public static IScalarQuery<TSource> ElementAt<TSource>(this IQuery<TSource> source, int index) =>
        source.ApplyOperation(x => x.ElementAt(index));

    public static IScalarQuery<TSource> ElementAtOrDefault<TSource>(this IQuery<TSource> source, int index) =>
        source.ApplyOperation(x => x.ElementAtOrDefault(index));

    public static IScalarQuery<bool> Contains<TSource>(this IQuery<TSource> source, TSource item) =>
        source.ApplyOperation(x => x.Contains(item));

    public static IScalarQuery<bool> Contains<TSource>(this IQuery<TSource> source, TSource item, IEqualityComparer<TSource>? comparer) =>
        source.ApplyOperation(x => x.Contains(item, comparer));

    public static IScalarQuery<bool> SequenceEqual<TSource>(this IQuery<TSource> source1, IEnumerable<TSource> source2) =>
        source1.ApplyOperation(x => x.SequenceEqual(source2));

    public static IScalarQuery<bool> SequenceEqual<TSource>(this IQuery<TSource> source1, IEnumerable<TSource> source2, IEqualityComparer<TSource>? comparer) =>
        source1.ApplyOperation(x => x.SequenceEqual(source2, comparer));

    public static IScalarQuery<TResult> Min<TSource, TResult>(this IQuery<TSource> source, Expression<Func<TSource, TResult>> selector) =>
        source.ApplyOperation(x => x.Min(selector));

    public static IScalarQuery<TSource> Max<TSource>(this IQuery<TSource> source) =>
        source.ApplyOperation(x => x.Max());

    public static IScalarQuery<TResult> Max<TSource, TResult>(this IQuery<TSource> source, Expression<Func<TSource, TResult>> selector) =>
        source.ApplyOperation(x => x.Max(selector));

    public static IScalarQuery<int> Sum(this IQuery<int> source) =>
        source.ApplyOperation(x => x.Sum());

    public static IScalarQuery<int?> Sum(this IQuery<int?> source) =>
        source.ApplyOperation(x => x.Sum());

    public static IScalarQuery<long> Sum(this IQuery<long> source) =>
        source.ApplyOperation(x => x.Sum());

    public static IScalarQuery<long?> Sum(this IQuery<long?> source) =>
        source.ApplyOperation(x => x.Sum());

    public static IScalarQuery<float> Sum(this IQuery<float> source) =>
        source.ApplyOperation(x => x.Sum());

    public static IScalarQuery<float?> Sum(this IQuery<float?> source) =>
        source.ApplyOperation(x => x.Sum());

    public static IScalarQuery<double> Sum(this IQuery<double> source) =>
        source.ApplyOperation(x => x.Sum());

    public static IScalarQuery<double?> Sum(this IQuery<double?> source) =>
        source.ApplyOperation(x => x.Sum());

    public static IScalarQuery<decimal> Sum(this IQuery<decimal> source) =>
        source.ApplyOperation(x => x.Sum());

    public static IScalarQuery<decimal?> Sum(this IQuery<decimal?> source) =>
        source.ApplyOperation(x => x.Sum());

    public static IScalarQuery<int> Sum<TSource>(this IQuery<TSource> source, Expression<Func<TSource, int>> selector) =>
        source.ApplyOperation(x => x.Sum(selector));

    public static IScalarQuery<int?> Sum<TSource>(this IQuery<TSource> source, Expression<Func<TSource, int?>> selector) =>
        source.ApplyOperation(x => x.Sum(selector));

    public static IScalarQuery<long> Sum<TSource>(this IQuery<TSource> source, Expression<Func<TSource, long>> selector) =>
        source.ApplyOperation(x => x.Sum(selector));

    public static IScalarQuery<long?> Sum<TSource>(this IQuery<TSource> source, Expression<Func<TSource, long?>> selector) =>
        source.ApplyOperation(x => x.Sum(selector));

    public static IScalarQuery<float> Sum<TSource>(this IQuery<TSource> source, Expression<Func<TSource, float>> selector) =>
        source.ApplyOperation(x => x.Sum(selector));

    public static IScalarQuery<float?> Sum<TSource>(this IQuery<TSource> source, Expression<Func<TSource, float?>> selector) =>
        source.ApplyOperation(x => x.Sum(selector));

    public static IScalarQuery<double> Sum<TSource>(this IQuery<TSource> source, Expression<Func<TSource, double>> selector) =>
        source.ApplyOperation(x => x.Sum(selector));

    public static IScalarQuery<double?> Sum<TSource>(this IQuery<TSource> source, Expression<Func<TSource, double?>> selector) =>
        source.ApplyOperation(x => x.Sum(selector));

    public static IScalarQuery<decimal> Sum<TSource>(this IQuery<TSource> source, Expression<Func<TSource, decimal>> selector) =>
        source.ApplyOperation(x => x.Sum(selector));

    public static IScalarQuery<decimal?> Sum<TSource>(this IQuery<TSource> source, Expression<Func<TSource, decimal?>> selector) =>
        source.ApplyOperation(x => x.Sum(selector));

    public static IScalarQuery<double> Average(this IQuery<int> source) =>
        source.ApplyOperation(x => x.Average());

    public static IScalarQuery<double?> Average(this IQuery<int?> source) =>
        source.ApplyOperation(x => x.Average());

    public static IScalarQuery<double> Average(this IQuery<long> source) =>
        source.ApplyOperation(x => x.Average());

    public static IScalarQuery<double?> Average(this IQuery<long?> source) =>
        source.ApplyOperation(x => x.Average());

    public static IScalarQuery<float> Average(this IQuery<float> source) =>
        source.ApplyOperation(x => x.Average());

    public static IScalarQuery<float?> Average(this IQuery<float?> source) =>
        source.ApplyOperation(x => x.Average());

    public static IScalarQuery<double> Average(this IQuery<double> source) =>
        source.ApplyOperation(x => x.Average());

    public static IScalarQuery<double?> Average(this IQuery<double?> source) =>
        source.ApplyOperation(x => x.Average());

    public static IScalarQuery<decimal> Average(this IQuery<decimal> source) =>
        source.ApplyOperation(x => x.Average());

    public static IScalarQuery<decimal?> Average(this IQuery<decimal?> source) =>
        source.ApplyOperation(x => x.Average());

    public static IScalarQuery<double> Average<TSource>(this IQuery<TSource> source, Expression<Func<TSource, int>> selector) =>
        source.ApplyOperation(x => x.Average(selector));

    public static IScalarQuery<double?> Average<TSource>(this IQuery<TSource> source, Expression<Func<TSource, int?>> selector) =>
        source.ApplyOperation(x => x.Average(selector));

    public static IScalarQuery<float> Average<TSource>(this IQuery<TSource> source, Expression<Func<TSource, float>> selector) =>
        source.ApplyOperation(x => x.Average(selector));

    public static IScalarQuery<float?> Average<TSource>(this IQuery<TSource> source, Expression<Func<TSource, float?>> selector) =>
        source.ApplyOperation(x => x.Average(selector));

    public static IScalarQuery<double> Average<TSource>(this IQuery<TSource> source, Expression<Func<TSource, long>> selector) =>
        source.ApplyOperation(x => x.Average(selector));

    public static IScalarQuery<double?> Average<TSource>(this IQuery<TSource> source, Expression<Func<TSource, long?>> selector) =>
        source.ApplyOperation(x => x.Average(selector));

    public static IScalarQuery<double> Average<TSource>(this IQuery<TSource> source, Expression<Func<TSource, double>> selector) =>
        source.ApplyOperation(x => x.Average(selector));

    public static IScalarQuery<double?> Average<TSource>(this IQuery<TSource> source, Expression<Func<TSource, double?>> selector) =>
        source.ApplyOperation(x => x.Average(selector));

    public static IScalarQuery<decimal> Average<TSource>(this IQuery<TSource> source, Expression<Func<TSource, decimal>> selector) =>
        source.ApplyOperation(x => x.Average(selector));

    public static IScalarQuery<decimal?> Average<TSource>(this IQuery<TSource> source, Expression<Func<TSource, decimal?>> selector) =>
        source.ApplyOperation(x => x.Average(selector));

    public static IScalarQuery<TSource> Aggregate<TSource>(this IQuery<TSource> source, Expression<Func<TSource, TSource, TSource>> func) =>
        source.ApplyOperation(x => x.Aggregate(func));

    public static IScalarQuery<TAccumulate> Aggregate<TSource, TAccumulate>(this IQuery<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func) =>
        source.ApplyOperation(x => x.Aggregate(seed, func));

    public static IScalarQuery<TResult> Aggregate<TSource, TAccumulate, TResult>(this IQuery<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func, Expression<Func<TAccumulate, TResult>> selector) =>
        source.ApplyOperation(x => x.Aggregate(seed, func, selector));
}
