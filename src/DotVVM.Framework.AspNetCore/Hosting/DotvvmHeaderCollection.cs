using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHeaderCollection : IHeaderCollection
    {
        public DotvvmHeaderCollection(IHeaderDictionary originalHeaders)
        {
            OriginalHeaders = originalHeaders;
        }

        public IHeaderDictionary OriginalHeaders { get; }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<string, string>>) OriginalHeaders.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return OriginalHeaders.GetEnumerator();
        }

        public void Add(KeyValuePair<string, string> item)
        {
            OriginalHeaders.Add(this.ConvertKeyValuePair(item));
        }

        public void Clear()
        {
            OriginalHeaders.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return OriginalHeaders.Contains(this.ConvertKeyValuePair(item));
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return OriginalHeaders.Remove(ConvertKeyValuePair(item));
        }

        public int Count => OriginalHeaders.Count;

        public bool IsReadOnly => OriginalHeaders.IsReadOnly;
        public void Add(string key, string value)
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

        public bool TryGetValue(string key, out string value)
        {
            StringValues tmp;
            var result = OriginalHeaders.TryGetValue(key, out tmp);
            value = tmp.ToString();
            return result;
        }

        public string this[string key]
        {
            get { return OriginalHeaders[key]; }
            set { OriginalHeaders[key] = value; }
        }

        public ICollection<string> Keys => OriginalHeaders.Keys;

        public ICollection<string> Values
        {
            get { return OriginalHeaders.Values.Select(e => e.ToString()).ToList(); }
        }

        protected KeyValuePair<string, StringValues> ConvertKeyValuePair(KeyValuePair<string, string> toConvert)
        {
            return new KeyValuePair<string, StringValues>(toConvert.Key, toConvert.Value);
        }
    }
}