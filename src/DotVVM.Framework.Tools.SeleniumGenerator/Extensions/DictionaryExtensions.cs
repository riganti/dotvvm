using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> AddRange<TKey, TValue>(this Dictionary<TKey, TValue> first,
            Dictionary<TKey, TValue> second)
        {
            return first.Union(second).ToDictionary(t => t.Key, t => t.Value);
        }
    }
}