using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Testing.Generator.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> AddRange<TKey, TValue>(this Dictionary<TKey, TValue> first,
            Dictionary<TKey, TValue> second)
        {
            return first.Union(second).ToDictionary(t => t.Key, t => t.Value);
        }
    }
    public static class ServiceProviderExtensions
    {
        public static T TryGetService<T>(this IServiceProvider serviceProvider)
        {
            try
            {
                return serviceProvider.GetService<T>();
            }
            catch
            {
                return default;
            }
        }
    }
}
