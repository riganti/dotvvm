using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    public static class PagingImplementation
    {
        public static Func<IQueryable, CancellationToken, Task<int?>>? CustomAsyncQueryableCountDelegate;

        /// <summary>
        /// Applies paging to the <paramref name="queryable" /> after the total number
        /// of items is retrieved.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}" /> to modify.</param>
        public static IQueryable<T> ApplyPagingToQueryable<T, TPagingOptions>(IQueryable<T> queryable, TPagingOptions options)
            where TPagingOptions : IPagingPageSizeCapability, IPagingPageIndexCapability
        {
            return options.PageSize > 0
                ? queryable.Skip(options.PageSize * options.PageIndex).Take(options.PageSize)
                : queryable;
        }

#if NET6_0_OR_GREATER
        /// <summary> Attempts to count the queryable asynchronously. EF Core IQueryables are supported, and IQueryables which return IAsyncEnumerable from GroupBy operator also work correctly. Otherwise, a synchronous fallback is used, or <see cref="CustomAsyncQueryableCountDelegate" /> may be set to add support for an ORM mapper of choice. </summary>
        public static async Task<int> QueryableAsyncCount<T>(IQueryable<T> queryable, CancellationToken ct = default)
        {
            if (CustomAsyncQueryableCountDelegate is {} customDelegate)
            {
                var result = await customDelegate(queryable, ct);
                if (result.HasValue)
                {
                    return result.Value;
                }
            }

            var queryableType = queryable.GetType();
            // Note: there is not a standard way to get a count from IAsyncEnumerable instance, without enumerating it.
            //       we use two heuristics to try to get the count using the query provider:
            //          * if we detect usage of EF Core, call its CountAsync method
            //          * otherwise, do it as .GroupBy(_ => 1).Select(group => group.Count()).SingleOrDefault()
            //  (if you are reading this and need a separate hack for your favorite ORM, you can set
            //   CustomAsyncQueryableCountDelegate, and we do accept PRs adding new heuristics ;) )
            return await (
                EfCoreAsyncCountHack(queryable, queryableType, ct) ??
                Ef6AsyncCountHack(queryable, queryableType, ct) ??
                MartenAsyncCountHack(queryable, queryableType, ct) ??
                StandardAsyncCountHack(queryable, ct)
            );
        }

        static MethodInfo? efMethodCache;
        static Task<int>? EfCoreAsyncCountHack<T>(IQueryable<T> queryable, Type queryableType, CancellationToken ct)
        {
            if (!(
                queryableType.Namespace == "Microsoft.EntityFrameworkCore.Query.Internal" && queryableType.Name == "EntityQueryable`1" ||
                queryableType.Namespace == "Microsoft.EntityFrameworkCore.Internal" && queryableType.Name == "InternalDbSet`1"
            ))
                return null;

            var countMethod = efMethodCache ?? queryableType.Assembly.GetType("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions")!.GetMethods().SingleOrDefault(m => m.Name == "CountAsync" && m.GetParameters() is { Length: 2 } parameters && parameters[1].ParameterType == typeof(CancellationToken));
            if (countMethod is null)
                return null;

            if (efMethodCache is null)
                Interlocked.CompareExchange(ref efMethodCache, countMethod, null);

            var countMethodGeneric = countMethod.MakeGenericMethod(typeof(T));
            return (Task<int>)countMethodGeneric.Invoke(null, new object[] { queryable, ct })!;
        }

        static MethodInfo? ef6CountMethodCache;
        static Task<int>? Ef6AsyncCountHack<T>(IQueryable<T> queryable, Type queryableType, CancellationToken ct)
        {
            var providerType = queryable.Provider.GetType();
            if (providerType.Namespace?.StartsWith("System.Data.Entity") != true)
                 return null;

            var countMethod = ef6CountMethodCache ?? providerType.Assembly.GetType("System.Data.Entity.QueryableExtensions")?.GetMethods().SingleOrDefault(m => 
                m.Name == "CountAsync" && 
                m.IsGenericMethodDefinition && 
                m.GetGenericArguments().Length == 1 &&
                m.GetParameters() is { Length: 2 } parameters && 
                parameters[0].ParameterType.IsGenericType &&
                parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>) &&
                parameters[1].ParameterType == typeof(CancellationToken));
            if (countMethod is null)
                return null;

            if (ef6CountMethodCache is null)
                Interlocked.CompareExchange(ref ef6CountMethodCache, countMethod, null);

            var countMethodGeneric = countMethod.MakeGenericMethod(typeof(T));
            return (Task<int>)countMethodGeneric.Invoke(null, new object[] { queryable, ct })!;
        }

        static MethodInfo? martenMethodCache;
        static Task<int>? MartenAsyncCountHack<T>(IQueryable<T> queryable, Type queryableType, CancellationToken ct)
        {
            if (!(queryableType.Namespace == "Marten.Linq" && queryableType.Name == "MartenLinqQueryable`1"))
                return null;

            var countMethod = martenMethodCache ?? queryableType.Assembly.GetType("Marten.QueryableExtensions")!.GetMethods().SingleOrDefault(m => m.Name == "CountAsync" && m.GetParameters() is { Length: 2 } parameters && parameters[1].ParameterType == typeof(CancellationToken));
            if (countMethod is null)
                return null;

            if (martenMethodCache is null)
                Interlocked.CompareExchange(ref martenMethodCache, countMethod, null);

            var countMethodGeneric = countMethod.MakeGenericMethod(typeof(T));
            return (Task<int>)countMethodGeneric.Invoke(null, new object[] { queryable, ct })!;
        }

        static Task<int> StandardAsyncCountHack<T>(IQueryable<T> queryable, CancellationToken ct)
        {
#if NETSTANDARD2_1_OR_GREATER
            var countGroupHack = queryable.GroupBy(_ => 1).Select(group => group.Count());
            // if not IAsyncEnumerable, just use synchronous Count on a new thread
            if (countGroupHack is not IAsyncEnumerable<int> countGroupEnumerable)
            {
                return Task.Factory.StartNew(() => queryable.Count(), TaskCreationOptions.LongRunning);
            }

            return FirstOrDefaultAsync(countGroupEnumerable, ct);
        }

        static async Task<T?> FirstOrDefaultAsync<T>(IAsyncEnumerable<T> enumerable, CancellationToken ct)
        {
            await using var enumerator = enumerable.GetAsyncEnumerator(ct);
            return await enumerator.MoveNextAsync() ? enumerator.Current : default;
#else
            throw new Exception("IAsyncEnumerable is not supported on .NET Framework and the queryable does not support EntityFramework CountAsync.");
#endif
        }
#endif
    }
}
