using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
    public static class FunctionalExtensions
    {
        public static TValue GetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
            => dictionary[key];

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                return default(TValue);
            }
            return value;
        }

        public static TTarget ApplyAction<TTarget>(this TTarget target, Action<TTarget> outerAction)
        {
            outerAction(target);
            return target;
        }

        public static TResult Apply<TTarget, TResult>(this TTarget target, Func<TTarget, TResult> outerFunction)
            => outerFunction(target);

        public static TOut CastTo<TOut>(this object original)
            where TOut : class
            => (TOut)original;

        public static TOut As<TOut>(this object original)
            where TOut : class
            => original as TOut;     
    }
}
