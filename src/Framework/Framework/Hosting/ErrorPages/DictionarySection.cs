using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class DictionarySection<K, V> : IErrorSectionFormatter
    {
        public string DisplayName { get; set; }

        public string Id { get; set; }
        public KeyValuePair<K, V>[] Table { get; set; }

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
