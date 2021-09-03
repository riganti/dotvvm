using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;

namespace DotVVM.Framework.Utils
{
    internal class ReadOnlyDictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private ConcurrentDictionary<TKey, TValue> dictionary;

        public ReadOnlyDictionaryWrapper(ConcurrentDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
        }

        public TValue this[TKey key] => dictionary[key];

        public IEnumerable<TKey> Keys => dictionary.Keys;

        public IEnumerable<TValue> Values => dictionary.Values;

        public int Count => dictionary.Count;

        public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();

        public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => dictionary.GetEnumerator();



        public static IReadOnlyDictionary<TKey, TValue> WrapIfNeeded(ConcurrentDictionary<TKey, TValue> dictionary)
        {
#if NETCORE
            return dictionary;
#else
            return new ReadOnlyDictionaryWrapper<TKey, TValue>(dictionary);
#endif
        }
    }
}
