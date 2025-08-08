using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class DictionarySection<K, V> : IErrorSectionFormatter
    {
        public string DisplayName { get; init; }
        public string KeyTitle { get; init; } = "Variable";
        public string ValueTitle { get; init; } = "Value";

        public string Id { get; init; }
        public KeyValuePair<K, V>[] Table { get; init; }

        public DictionarySection(string name, string id, IEnumerable<KeyValuePair<K, V>> table)
        {
            DisplayName = name;
            Id = id;
            Table = table.ToArray();
        }

        public void WriteBody(IErrorWriter writer)
        {
            writer.WriteKVTable(Table);
        }

        public void WriteStyle(IErrorWriter writer)
        {
        }
    }
}
