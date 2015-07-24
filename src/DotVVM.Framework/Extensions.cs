using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework
{
    public static class Extensions
    {

        public static IEnumerable<T> TakeWhileReverse<T>(this IList<T> items, Func<T, bool> predicate)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (!predicate(items[i]))
                {
                    yield break;
                }
                yield return items[i];
            }
        }

    }
}