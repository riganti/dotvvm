using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Testing
{
    internal class TestCookieCollection: Dictionary<string, string>, ICookieCollection
    {
        public TestCookieCollection() : base()
        {
        }

        public TestCookieCollection(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        public TestCookieCollection(IDictionary<string, string> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer)
        {
        }

        public TestCookieCollection(IEqualityComparer<string> comparer) : base(comparer)
        {
        }

        public TestCookieCollection(int capacity) : base(capacity)
        {
        }

        public TestCookieCollection(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer)
        {
        }

        protected TestCookieCollection(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
