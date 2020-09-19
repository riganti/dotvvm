#nullable enable

using System.Collections.Concurrent;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Caching
{
    /// <summary>
    /// Simple implementation of LRU - a cache that remembers only the least recently used items. This dictionary has size between generationSize and 2*generationSize.
    /// It also automatically removes entries older than 2*generationTickTime
    /// Actually, the entries are not removed, the reference is just weakened, so GC can collect it. If the object is actually from another place, it will stay in the cache.
    /// </summary>
    public class SimpleLruDictionary<TKey, TValue>
        where TValue : class
    {
        // new generation
        private ConcurrentDictionary<TKey, TValue> hot = new ConcurrentDictionary<TKey, TValue>();
        // old generation
        private ConcurrentDictionary<TKey, TValue> cold = new ConcurrentDictionary<TKey, TValue>();
        // free to take for GC. however, if the GC does not want to collect, we can still use it
        private readonly ConcurrentDictionary<TKey, WeakReference<TValue>> dead = new ConcurrentDictionary<TKey, WeakReference<TValue>>();
        private TimeSpan lastCleanupTime = TimeSpan.MinValue;

        private readonly int generationSize;
        private readonly TimeSpan generationTickTime;
        private readonly Stopwatch stopWatch = Stopwatch.StartNew(); // this is more reliable source of time information than DateTime.Now (due to system-wide time change, ...)

        public SimpleLruDictionary(int generationSize, TimeSpan generationTickTime)
        {
            this.generationSize = generationSize;
            this.generationTickTime = generationTickTime;
            Task.Factory.StartNew(SetupTimer); 
        }

        private object locker = new object();
        internal bool Cleanup(int minSize, TimeSpan minTime)
        {
            lock (locker)
            {
                // check that's still needed
                if (minSize > hot.Count) return false;
                if (minTime > lastCleanupTime) return false;

                lastCleanupTime = stopWatch.Elapsed;

                foreach (var i in cold)
                {
                    dead.TryAdd(i.Key, new WeakReference<TValue>(i.Value));
                }
                var newHot = new ConcurrentDictionary<TKey, TValue>();
                cold = hot;
                hot = newHot;
            }
            CleanDeadReferences();
            return true;
        }

        private void CleanDeadReferences()
        {
            foreach (var i in dead)
            {
                if (!i.Value.TryGetTarget(out var _))
                {
                    dead.TryRemove(i.Key, out var _);
                    // at this point, I could have removed different item than what I have checked. But it was marked dead anyway, so it won't really corrupt anything
                    // (i.e. we won't have two different items, just drop item that should not have been really dropped)
                }
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (hot.TryGetValue(key, out value))
                return true;
            lock (locker)
            {
                if (hot.TryGetValue(key, out value))
                    return true;
                if (cold.TryGetValue(key, out value))
                {
                    // add back to `hot` -- this way, we mark the item as used
                    hot.TryAdd(key, value);
                    return true;
                }
                if (dead.TryGetValue(key, out var valueRef))
                {
                    if (valueRef.TryGetTarget(out value))
                    {
                        // add back to `hot` -- this way, we mark the item as used
                        hot.TryAdd(key, value);
                        return true;
                    }
                    else
                    {
                        dead.TryRemove(key, out var _);
                    }
                }
                return false;
            }
        }

        public void TryAdd(TKey key, Func<TKey, TValue> factory) => GetOrCreate(key, factory);

        public TValue GetOrCreate(TKey key, Func<TKey, TValue> factory)
        {
            if (TryGetValue(key, out var value) && value is object) return value;

            if (this.hot.Count >= this.generationSize)
            {
                Cleanup(minSize: this.generationSize, minTime: TimeSpan.MinValue);
                // this took some time, let's check the cache again
                if (TryGetValue(key, out value) && value is object) return value;
            }

            // TODO: do we want to lock the creation?
            var newValue = factory(key);
            if (newValue == null) return default;
            this.hot.TryAdd(key, newValue);
            // don't return the newValue - in case was another one created in parallel, we want to return the older one
            // so only one permanent instance of this kind is created.
            return GetOrCreate(key, factory);
        }

        public bool Remove(TKey key, out TValue oldValue)
        {
            lock (locker)
            {
                var r = false;
                oldValue = default!;
                if (hot.TryRemove(key, out var hotValue))
                {
                    oldValue = hotValue;
                    r = true;
                }
                if (cold.TryRemove(key, out var coldValue))
                {
                    oldValue = coldValue;
                    r = true;
                }
                if (dead.TryRemove(key, out var deadValueWR) && deadValueWR.TryGetTarget(out var deadValue))
                {
                    oldValue = deadValue;
                    r = true;
                }
                return r;
            }
        }

        void SetupTimer()
        {
            static TimeSpan? invokeTimer(WeakReference<SimpleLruDictionary<TKey, TValue>> weakThis, TimeSpan lastTime)
            {
                if (weakThis.TryGetTarget(out var @this))
                {
                    @this.Cleanup(minSize: 0, minTime: lastTime);
                    return @this.lastCleanupTime;
                }
                else
                {
                    return null;
                }
            }
            static async void timerCore(TimeSpan tickTime, TimeSpan lastTime, WeakReference<SimpleLruDictionary<TKey, TValue>> @this)
            {
                while (true)
                {
                    await Task.Delay(tickTime);
                    var r = invokeTimer(@this, lastTime);
                    if (r == null) return; // the instance has died, kill the timer
                    lastTime = r.Value;
                }
            }

            timerCore(
                this.generationTickTime,
                this.lastCleanupTime,
                new WeakReference<SimpleLruDictionary<TKey, TValue>>(this)
            );
        }
    }
}
