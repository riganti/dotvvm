using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Utils
{
    public class TupleComparer<T1, T2> : IEqualityComparer<(T1, T2)>
    {
        private readonly IEqualityComparer<T1>? comparer1;
        private readonly IEqualityComparer<T2>? comparer2;

        public TupleComparer(IEqualityComparer<T1>? comparer1, IEqualityComparer<T2>? comparer2)
        {
            this.comparer1 = comparer1;
            this.comparer2 = comparer2;
        }

        public bool Equals((T1, T2) x, (T1, T2) y) =>
            (comparer1?.Equals(x.Item1, y.Item1) ?? x.Item1?.Equals(y.Item1) ?? y.Item1 == null) &&
            (comparer2?.Equals(x.Item2, y.Item2) ?? x.Item2?.Equals(y.Item2) ?? y.Item2 == null);

        public int GetHashCode((T1, T2) x) =>
            (x.Item1 == null ? 1906945227 : comparer1?.GetHashCode(x.Item1) ?? x.Item1.GetHashCode()) * 101265739 ^
            (x.Item2 == null ? 0652178751 : comparer2?.GetHashCode(x.Item2) ?? x.Item2.GetHashCode()) * 540893449;
    }
}
