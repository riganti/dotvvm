using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHeaderCollection : IHeaderCollection
    {
        public DotvvmHeaderCollection(IHeaderDictionary originalHeaders)
        {
            OriginalHeaders = originalHeaders;
        }

        public IHeaderDictionary OriginalHeaders { get; }

        public void Append(string key, string value)
        {
            OriginalHeaders.Append(key, value);
        }


        public string this[string key]
        {
            get { return OriginalHeaders[key]; }
            set { OriginalHeaders[key] = value; }
        }


        #region IDictionary bridge
        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return OriginalHeaders.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return OriginalHeaders.GetEnumerator();
        }

        public void Add(KeyValuePair<string, string[]> item)
        {
            OriginalHeaders.Add(item);
        }

        public void Clear()
        {
            OriginalHeaders.Clear();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            return OriginalHeaders.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            OriginalHeaders.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            return OriginalHeaders.Remove(item);
        }

        public int Count => OriginalHeaders.Count;

        public bool IsReadOnly => OriginalHeaders.IsReadOnly;
        public void Add(string key, string[] value)
        {
            OriginalHeaders.Add(key, value);
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
            return OriginalHeaders.TryGetValue(key, out value);
        }

        public ICollection<string> Keys => OriginalHeaders.Keys;

        public ICollection<string[]> Values
        {
            get { return OriginalHeaders.Values; }
        }

        string[] IDictionary<string, string[]>.this[string key]
        {
            get { return ((IDictionary<string, string[]>)OriginalHeaders)[key]; } 
            set { ((IDictionary<string, string[]>)OriginalHeaders)[key] = value; }
        }
        #endregion
    }
}