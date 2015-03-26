using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Utils
{
    static class CollectionUtils
    {
        public static int PushRange<T>(this Stack<T> stack, IEnumerable<T> items)
        {
            int count = 0;
            foreach (var item in items)
            {
                stack.Push(item);
                count++;
            }
            return count;
        }

        public static void PopMultiple<T>(this Stack<T> stack, int count)
        {
            for (int i = 0; i < count; i++)
            {
                stack.Pop();
            }
        }
    }
}
