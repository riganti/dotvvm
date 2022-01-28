using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Testing
{
    public class TestQueryCollection: Dictionary<string, string>, IQueryCollection, IEquatable<TestQueryCollection>
    {
        public TestQueryCollection() : base()
        {
        }

        public TestQueryCollection(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        public TestQueryCollection(IDictionary<string, string> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer)
        {
        }

        public TestQueryCollection(IEqualityComparer<string> comparer) : base(comparer)
        {
        }

        public TestQueryCollection(int capacity) : base(capacity)
        {
        }

        public TestQueryCollection(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer)
        {
        }

        protected TestQueryCollection(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public bool Equals(TestQueryCollection? other) =>
            Object.ReferenceEquals(this, other) ||
            other != null && other.OrderBy(k => k.Key).SequenceEqual(this.OrderBy(k => k.Key));
    }
}
