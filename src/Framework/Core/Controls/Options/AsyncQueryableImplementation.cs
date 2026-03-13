using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    public static class AsyncQueryableImplementation
    {
        public static async Task<IReadOnlyList<T>> QueryableToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken)
        {
            if (queryable is IAsyncEnumerable<T> asyncPaged)
            {
                // use IAsyncEnumerable implementation
                var result = new List<T>();
                await foreach (var item in asyncPaged.WithCancellation(cancellationToken))
                {
                    result.Add(item);
                }
                return result;
            }

            var queryableType = queryable.GetType();
            
            // EF6 DbQuery support
            if (queryableType is { Namespace: "System.Data.Entity.Infrastructure", Name: "DbQuery`1" })
            {
                return await Ef6ToListAsync(queryable, queryableType, cancellationToken);
            }
            
            // Marten support
            if (queryableType is { Namespace: "Marten.Linq", Name: "MartenLinqQueryable`1" })
            {
                return await MartenToListAsync(queryable, queryableType, cancellationToken);
            }

            throw new ArgumentException($"The specified IQueryable ({queryable.GetType().FullName}), does not support async enumeration. Please use the LoadFromQueryable method.", nameof(queryable));
        }


        static MethodInfo? ef6MethodCache;
        private static Task<List<T>> Ef6ToListAsync<T>(IQueryable<T> queryable, Type queryableType, CancellationToken ct)
        {
            var toListAsyncMethod = ef6MethodCache
                ?? queryableType.Assembly.GetType("System.Data.Entity.QueryableExtensions")?.GetMethods().SingleOrDefault(m => 
                    m.Name == "ToListAsync" && 
                    m.IsGenericMethodDefinition && 
                    m.GetGenericArguments().Length == 1 &&
                    m.GetParameters() is { Length: 2 } parameters && 
                    parameters[0].ParameterType.IsGenericType &&
                    parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>) &&
                    parameters[1].ParameterType == typeof(CancellationToken))
                ?? throw new InvalidOperationException("Entity Framework 6 ToListAsync method not found.");

            if (ef6MethodCache is null)
                Interlocked.CompareExchange(ref ef6MethodCache, toListAsyncMethod, null);

            var toListMethodGeneric = toListAsyncMethod.MakeGenericMethod(typeof(T));
            var result = toListMethodGeneric.Invoke(null, [queryable, ct])!;
            return (Task<List<T>>)result;
        }

        static MethodInfo? martenMethodCache;
        private static Task<IReadOnlyList<T>> MartenToListAsync<T>(IQueryable<T> queryable, Type queryableType, CancellationToken ct)
        {
            var toListAsyncMethod = martenMethodCache
                ?? queryableType.Assembly.GetType("Marten.QueryableExtensions")!.GetMethods()
                    .SingleOrDefault(m => m.Name == "ToListAsync"
                        && m.GetParameters() is { Length: 2 } parameters
                        && parameters[1].ParameterType == typeof(CancellationToken))
                ?? throw new InvalidOperationException("Marten's ToListAsync method not found.");

            if (martenMethodCache is null)
                Interlocked.CompareExchange(ref martenMethodCache, toListAsyncMethod, null);

            var toListMethodGeneric = toListAsyncMethod.MakeGenericMethod(typeof(T));
            var result = toListMethodGeneric.Invoke(null, [queryable, ct])!;
            return (Task<IReadOnlyList<T>>)result;
        }
    }
}
