using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHeaderCollection : IHeaderCollection
    {
        public DotvvmHeaderCollection(IHeaderDictionary originalHeaders)
        {
            OriginalHeaders = originalHeaders;
        }

        public IHeaderDictionary OriginalHeaders { get; }

        public string this[string key]
        {
            get { return OriginalHeaders[key]; }
            set { OriginalHeaders[key] = value; }
        }

        public void Append(string key, string value)
        {
            OriginalHeaders[key] = OriginalHeaders[key].Concat(new[] { value }).ToArray();
        }

        #region IDictionary bridge
        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            foreach (var item in OriginalHeaders)
            {
                yield return new KeyValuePair<string, string[]>(item.Key, item.Value.ToArray());
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(KeyValuePair<string, string[]> item)
        {
            OriginalHeaders.Add(ConvertKeyValuePair(item));
        }

        public void Clear()
        {
            OriginalHeaders.Clear();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            return OriginalHeaders.Contains(ConvertKeyValuePair(item));
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            return OriginalHeaders.Remove(ConvertKeyValuePair(item));
        }

        public int Count => OriginalHeaders.Count;

        public bool IsReadOnly => OriginalHeaders.IsReadOnly;
        public void Add(string key, string[] value)
        {
            OriginalHeaders.Add(key, new StringValues(value));
        }

        public bool ContainsKey(string key)
        {
            return OriginalHeaders.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return OriginalHeaders.Remove(key);
        }

        public bool TryGetValue(string key, out string[] value)
        {
            StringValues tmp;
            var result = OriginalHeaders.TryGetValue(key, out tmp);
            if (result) value = tmp.ToArray();
            else value = null;
            return result;
        }

        public ICollection<string> Keys => OriginalHeaders.Keys;

        public ICollection<string[]> Values
        {
            get { return OriginalHeaders.Values.Select(e => e.ToArray()).ToList(); }
        }

        string[] IDictionary<string, string[]>.this[string key]
        {
            get { return ((IDictionary<string, string[]>)OriginalHeaders)[key]; }
            set { ((IDictionary<string, string[]>)OriginalHeaders)[key] = value; }
        }

        protected static KeyValuePair<string, StringValues> ConvertKeyValuePair(KeyValuePair<string, string[]> toConvert) => 
            new KeyValuePair<string, StringValues>(toConvert.Key, new StringValues(toConvert.Value));
        #endregion
    }
}