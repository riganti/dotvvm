using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> Union<TKey, TValue>(this Dictionary<TKey, TValue> first,
            Dictionary<TKey, TValue> second)
        {
            return Enumerable.Union(first, second).ToDictionary(t => t.Key, t => t.Value);
        }
    }
}
