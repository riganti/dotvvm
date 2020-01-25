#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Testing
{
    internal class TestHeaderCollection : Dictionary<string, string[]>, IHeaderCollection
    {
        public TestHeaderCollection() : base()
        {
        }

        public TestHeaderCollection(IDictionary<string, string[]> dictionary) : base(dictionary)
        {
        }

        public TestHeaderCollection(IDictionary<string, string[]> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer)
        {
        }

        public TestHeaderCollection(IEqualityComparer<string> comparer) : base(comparer)
        {
        }

        public TestHeaderCollection(int capacity) : base(capacity)
        {
        }

        public TestHeaderCollection(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer)
        {
        }

        protected TestHeaderCollection(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        string IHeaderCollection.this[string key]
        {
            get => string.Join("; ", this[key]);
            set => this[key] = new string[] { value };
        }

        public void Append(string key, string value)
        {
            if (this.TryGetValue(key, out var currentVal))
                this[key] = currentVal.Concat(new [] { value }).ToArray();
            else
                this[key] = new [] { value };
        }
    }
}
