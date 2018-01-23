using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using AspQueryCollection = Microsoft.AspNetCore.Http.IQueryCollection;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmQueryCollection : IQueryCollection
    {
        public DotvvmQueryCollection(AspQueryCollection originalQuery)
        {
            OriginalQuery = originalQuery;
        }

        public AspQueryCollection OriginalQuery { get; }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var q in this.OriginalQuery)
            {
                foreach (var str in q.Value)
                {
                    yield return new KeyValuePair<string, string>(q.Key, str);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string this[string key] => OriginalQuery[key];

        public bool TryGetValue(string key, out string value)
        {
            var result = OriginalQuery.TryGetValue(key, out var values);
            value = values.ToString();
            return result;
        }

        public bool ContainsKey(string key)
        {
            return OriginalQuery.ContainsKey(key);
        }
    }
}