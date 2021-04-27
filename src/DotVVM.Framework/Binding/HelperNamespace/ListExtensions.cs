using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    public static class ListExtensions
    {
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
    }
}
