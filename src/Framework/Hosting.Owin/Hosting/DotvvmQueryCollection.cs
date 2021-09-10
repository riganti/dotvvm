using System.Collections;
using System.Collections.Generic;
using Microsoft.Owin;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmQueryCollection : IQueryCollection
    {
        public DotvvmQueryCollection(IReadableStringCollection originalQuery)
        {
            OriginalQuery = originalQuery;
        }

        public IReadableStringCollection OriginalQuery { get; }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<string, string>>) OriginalQuery.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string this[string key] => OriginalQuery[key];

        public bool TryGetValue(string key, out string value)
        {
            var result = OriginalQuery.GetValues(key);
            if (result == null)
            {
                value = null;
                return false;
            }
            else
            {
                value = string.Join(",", result);
                return true;
            }
        }

        public bool ContainsKey(string key)
        {
            return OriginalQuery.GetValues(key) != null;
        }
    }
}