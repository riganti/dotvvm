using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class DictionarySection : IErrorSectionFormatter
    {
        public string DisplayName { get; set; }

        public string Id { get; set; }
        public IEnumerable Keys { get; set; }
        public IEnumerable Values { get; set; }

        public DictionarySection(string name, string id)
        {
            DisplayName = name;
            Id = id;
        }

        public static DictionarySection Create(string name, string id, IDictionary dictionary)
            => new DictionarySection(name, id)
            {
                Keys = dictionary.Keys,
                Values = dictionary.Values
            };

        public static DictionarySection Create<TKey, TValue>(string name, string id, IEnumerable<KeyValuePair<TKey, TValue>> kvps)
            => new DictionarySection(name, id)
            {
                Keys = kvps.Select(kvp => kvp.Key).ToArray(),
                Values = kvps.Select(kvp => kvp.Value).ToArray()
            };

        public void WriteBody(IErrorWriter writer)
        {
            writer.WriteKVTable(Keys, Values);
        }

        public void WriteHead(IErrorWriter writer)
        {
        }
    }
}