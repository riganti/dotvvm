using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    public static class ListExtensions
    {
        /// <summary> Updates all entries identified by <paramref name="matcher"/> using the <paramref name="updater"/>. If none match, the <paramref name="element"/> is appended to the list. </summary>
        public static void AddOrUpdate<T>(this List<T> list, T element, Func<T,bool> matcher, Func<T,T> updater)
        {
            var found = false;
            for (var index = 0; index < list.Count; index++)
            {
                if (!matcher(list[index]))
                    continue;

                found = true;
                list[index] = updater(list[index]);
            }

            if (!found)
                list.Add(element);
        }

        /// <summary> Removes the first entry identified by <paramref name="predicate"/>. </summary>
        public static void RemoveFirst<T>(this List<T> list, Func<T,bool> predicate)
        {
            for (var index = 0; index < list.Count; index++)
            {
                if (predicate(list[index]))
                {
                    list.RemoveAt(index);
                    return;
                }
            }
        }

        /// <summary> Removes the last entry identified by <paramref name="predicate"/>. </summary>
        public static void RemoveLast<T>(this List<T> list, Func<T, bool> predicate)
        {
            for (var index = list.Count - 1; index >= 0; index--)
            {
                if (predicate(list[index]))
                {
                    list.RemoveAt(index);
                    return;
                }
            }
        }
    }
}
