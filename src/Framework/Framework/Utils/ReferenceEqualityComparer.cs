using System.Collections.Generic;

namespace DotVVM.Framework.Utils
{
    internal class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => obj?.GetHashCode() ?? 0;
    }
}
