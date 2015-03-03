using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.VS2015Extension
{
    public class CachedValue<T> where T : class
    {

        private volatile T cache;
        private object locker = new object();

        public T GetOrRetrieve(Func<T> valueFactory)
        {
            if (cache == null)
            {
                lock (locker)
                {
                    if (cache == null)
                    {
                        cache = valueFactory();
                    }
                }
            }
            return cache;
        }

        public void ClearCachedValue()
        {
            lock (locker)
            {
                cache = null;
            }
        }

    }
}
