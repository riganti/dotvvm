using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Query;
public static partial class DotvvmFrameworkQueryExtensions
{
    public static IQuery<TSource> Where<TSource>(this IQuery<TSource> source, Expression<Func<TSource, bool>> predicate) =>
        source.ApplyQueryableTransform(q => q.Where(predicate));

    public static IQuery<TSource> Where<TSource>(this IQuery<TSource> source, Expression<Func<TSource, int, bool>> predicate) =>
        source.ApplyQueryableTransform(q => q.Where(predicate));

    public static IQuery<TResult> OfType<TResult>(this IQuery<object> source) =>
        source.ApplyQueryableTransform(q => q.OfType<TResult>());

    public static IQuery<TResult> Cast<TResult>(this IQuery<object> source) =>
        source.ApplyQueryableTransform(q => q.Cast<TResult>());

    public static IQuery<TResult> Select<TSource, TResult>(this IQuery<TSource> source, Expression<Func<TSource, TResult>> selector) =>
        source.ApplyQueryableTransform(q => q.Select(selector));

    public static IQuery<TResult> Select<TSource, TResult>(this IQuery<TSource> source, Expression<Func<TSource, int, TResult>> selector) =>
        source.ApplyQueryableTransform(q => q.Select(selector));

    public static IQuery<TResult> SelectMany<TSource, TResult>(this IQuery<TSource> source, Expression<Func<TSource, IEnumerable<TResult>>> selector) =>
        source.ApplyQueryableTransform(q => q.SelectMany(selector));

    public static IQuery<TResult> SelectMany<TSource, TResult>(this IQuery<TSource> source, Expression<Func<TSource, int, IEnumerable<TResult>>> selector) =>
        source.ApplyQueryableTransform(q => q.SelectMany(selector));

    public static IQuery<TResult> SelectMany<TSource, TCollection, TResult>(this IQuery<TSource> source, Expression<Func<TSource, int, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector) =>
        source.ApplyQueryableTransform(q => q.SelectMany(collectionSelector, resultSelector));

    public static IQuery<TResult> SelectMany<TSource, TCollection, TResult>(this IQuery<TSource> source, Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector) =>
        source.ApplyQueryableTransform(q => q.SelectMany(collectionSelector, resultSelector));

    public static IQuery<TResult> Join<TOuter, TInner, TKey, TResult>(this IQuery<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector) =>
        outer.ApplyQueryableTransform(q => q.Join(inner, outerKeySelector, innerKeySelector, resultSelector));

    public static IQuery<TResult> Join<TOuter, TInner, TKey, TResult>(this IQuery<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector, IEqualityComparer<TKey>? comparer) =>
        outer.ApplyQueryableTransform(q => q.Join(inner, outerKeySelector, innerKeySelector, resultSelector, comparer));

    public static IQuery<TResult> Join<TOuter, TInner, TKey, TResult>(this IQuery<TOuter> outer, IQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector) =>
        outer.ApplyQueryableTransform(q => q.Join(inner.AsNonExecutableQueryable(), outerKeySelector, innerKeySelector, resultSelector));

    public static IQuery<TResult> Join<TOuter, TInner, TKey, TResult>(this IQuery<TOuter> outer, IQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector, IEqualityComparer<TKey>? comparer) =>
        outer.ApplyQueryableTransform(q => q.Join(inner.AsNonExecutableQueryable(), outerKeySelector, innerKeySelector, resultSelector, comparer));

    public static IQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IQuery<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector) =>
        outer.ApplyQueryableTransform(q => q.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector));

    public static IQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IQuery<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector, IEqualityComparer<TKey>? comparer) =>
        outer.ApplyQueryableTransform(q => q.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, comparer));

    public static IQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IQuery<TOuter> outer, IQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector) =>
        outer.ApplyQueryableTransform(q => q.GroupJoin(inner.AsNonExecutableQueryable(), outerKeySelector, innerKeySelector, resultSelector));

    public static IQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IQuery<TOuter> outer, IQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector, IEqualityComparer<TKey>? comparer) =>
        outer.ApplyQueryableTransform(q => q.GroupJoin(inner.AsNonExecutableQueryable(), outerKeySelector, innerKeySelector, resultSelector, comparer));

    public static IQuery<TSource> OrderBy<TSource, TKey>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector) =>
        source.ApplyQueryableTransform(q => q.OrderBy(keySelector));

    public static IQuery<TSource> OrderBy<TSource, TKey>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey>? comparer) =>
        source.ApplyQueryableTransform(q => q.OrderBy(keySelector, comparer));

    public static IQuery<TSource> OrderByDescending<TSource, TKey>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector) =>
        source.ApplyQueryableTransform(q => q.OrderByDescending(keySelector));

    public static IQuery<TSource> OrderByDescending<TSource, TKey>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey>? comparer) =>
        source.ApplyQueryableTransform(q => q.OrderByDescending(keySelector, comparer));

    // public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)


    // public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey>? comparer)


    // public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)


    // public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey>? comparer)


    public static IQuery<TSource> Take<TSource>(this IQuery<TSource> source, int count) =>
        source.ApplyQueryableTransform(q => q.Take(count));

    public static IQuery<TSource> TakeWhile<TSource>(this IQuery<TSource> source, Expression<Func<TSource, bool>> predicate) =>
        source.ApplyQueryableTransform(q => q.TakeWhile(predicate));

    public static IQuery<TSource> TakeWhile<TSource>(this IQuery<TSource> source, Expression<Func<TSource, int, bool>> predicate) =>
        source.ApplyQueryableTransform(q => q.TakeWhile(predicate));

    public static IQuery<TSource> Skip<TSource>(this IQuery<TSource> source, int count) =>
        source.ApplyQueryableTransform(q => q.Skip(count));
    public static IQuery<TSource> SkipWhile<TSource>(this IQuery<TSource> source, Expression<Func<TSource, bool>> predicate) =>
        source.ApplyQueryableTransform(q => q.SkipWhile(predicate));

    public static IQuery<TSource> SkipWhile<TSource>(this IQuery<TSource> source, Expression<Func<TSource, int, bool>> predicate) =>
        source.ApplyQueryableTransform(q => q.SkipWhile(predicate));

    public static IQuery<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector) =>
        source.ApplyQueryableTransform(q => q.GroupBy(keySelector));

    public static IQuery<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector) =>
        source.ApplyQueryableTransform(q => q.GroupBy(keySelector, elementSelector));

    public static IQuery<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector, IEqualityComparer<TKey>? comparer) =>
        source.ApplyQueryableTransform(q => q.GroupBy(keySelector, comparer));

    public static IQuery<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, IEqualityComparer<TKey>? comparer) =>
        source.ApplyQueryableTransform(q => q.GroupBy(keySelector, elementSelector, comparer));

    public static IQuery<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, Expression<Func<TKey, IEnumerable<TElement>, TResult>> resultSelector) =>
        source.ApplyQueryableTransform(q => q.GroupBy(keySelector, elementSelector, resultSelector));

    public static IQuery<TResult> GroupBy<TSource, TKey, TResult>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TKey, IEnumerable<TSource>, TResult>> resultSelector) =>
        source.ApplyQueryableTransform(q => q.GroupBy(keySelector, resultSelector));

    public static IQuery<TResult> GroupBy<TSource, TKey, TResult>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TKey, IEnumerable<TSource>, TResult>> resultSelector, IEqualityComparer<TKey>? comparer) =>
        source.ApplyQueryableTransform(q => q.GroupBy(keySelector, resultSelector, comparer));

    public static IQuery<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, Expression<Func<TKey, IEnumerable<TElement>, TResult>> resultSelector, IEqualityComparer<TKey>? comparer) =>
        source.ApplyQueryableTransform(q => q.GroupBy(keySelector, elementSelector, resultSelector, comparer));

    public static IQuery<TSource> Distinct<TSource>(this IQuery<TSource> source) =>
        source.ApplyQueryableTransform(q => q.Distinct());

    public static IQuery<TSource> Distinct<TSource>(this IQuery<TSource> source, IEqualityComparer<TSource>? comparer) =>
        source.ApplyQueryableTransform(q => q.Distinct(comparer));

    public static IQuery<TSource> Concat<TSource>(this IQuery<TSource> source1, IQuery<TSource> source2) =>
        source1.ApplyQueryableTransform(q => q.Concat(source2.AsNonExecutableQueryable()));

    public static IQuery<TResult> Zip<TFirst, TSecond, TResult>(this IQuery<TFirst> source1, IQuery<TSecond> source2, Expression<Func<TFirst, TSecond, TResult>> resultSelector) =>
        source1.ApplyQueryableTransform(q => q.Zip(source2.AsNonExecutableQueryable(), resultSelector));

    public static IQuery<TSource> Union<TSource>(this IQuery<TSource> source1, IQuery<TSource> source2) =>
        source1.ApplyQueryableTransform(q => q.Union(source2.AsNonExecutableQueryable()));

    public static IQuery<TSource> Union<TSource>(this IQuery<TSource> source1, IQuery<TSource> source2, IEqualityComparer<TSource>? comparer) =>
        source1.ApplyQueryableTransform(q => q.Union(source2.AsNonExecutableQueryable(), comparer));

    public static IQuery<TSource> Intersect<TSource>(this IQuery<TSource> source1, IQuery<TSource> source2)  =>
        source1.ApplyQueryableTransform(q => q.Intersect(source2.AsNonExecutableQueryable()));

    public static IQuery<TSource> Intersect<TSource>(this IQuery<TSource> source1, IQuery<TSource> source2, IEqualityComparer<TSource>? comparer) =>
        source1.ApplyQueryableTransform(q => q.Intersect(source2.AsNonExecutableQueryable(), comparer));


    public static IQuery<TSource> Except<TSource>(this IQuery<TSource> source1, IQuery<TSource> source2) =>
        source1.ApplyQueryableTransform(q => q.Except(source2.AsNonExecutableQueryable()));


    public static IQuery<TSource> Except<TSource>(this IQuery<TSource> source1, IQuery<TSource> source2, IEqualityComparer<TSource>? comparer) =>
        source1.ApplyQueryableTransform(q => q.Except(source2.AsNonExecutableQueryable(), comparer));
}
