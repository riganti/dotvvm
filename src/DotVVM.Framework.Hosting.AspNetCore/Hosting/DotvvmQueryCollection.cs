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
            return (IEnumerator<KeyValuePair<string, string>>) OriginalQuery.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string this[string key] => OriginalQuery[key];

        public bool TryGetValue(string key, out string value)
        {
            StringValues values;
            var result =  OriginalQuery.TryGetValue(key, out values);
            value = values.ToString();
            return result;
        }

        public bool ContainsKey(string key)
        {
            return OriginalQuery.ContainsKey(key);
        }
    }
}