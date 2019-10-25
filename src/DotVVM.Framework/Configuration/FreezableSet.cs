using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Configuration
{

    static class FreezableSet
    {
        public static void Freeze<T>(ref ISet<T> set)
        {
            if (set is FreezableSet<T> freezable)
                freezable.Freeze();
            else
            {
                var comparer = (set as HashSet<T>)?.Comparer;
                if (set is object && !set.IsReadOnly)
                    set = new FreezableSet<T>(set, comparer, frozen: true);
            }
        }
    }
    sealed class FreezableSet<T> : ISet<T>
    {
        private readonly HashSet<T> set;
        private bool isFrozen;

        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error("set");
        }
        public void Freeze()
        {
            this.isFrozen = true;
        }

        public FreezableSet(bool frozen = false, IEqualityComparer<T> comparer = null)
        {
            set = new HashSet<T>(comparer);
            isFrozen = frozen;
        }

        public FreezableSet(IEnumerable<T> items, IEqualityComparer<T> comparer = null, bool frozen = false)
        {
            set = new HashSet<T>(items, comparer);
            isFrozen = frozen;
        }


        public int Count => set.Count;

        public bool IsReadOnly => isFrozen;
        public bool Add(T item)
        {
            ThrowIfFrozen();
            return set.Add(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            ThrowIfFrozen();
            set.ExceptWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            ThrowIfFrozen();
            set.IntersectWith(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) => set.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => set.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => set.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => set.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => set.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => set.SetEquals(other);
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            ThrowIfFrozen();
            set.SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            ThrowIfFrozen();
            set.UnionWith(other);
        }

        void ICollection<T>.Add(T item) => this.Add(item);
        public void Clear()
        {
            ThrowIfFrozen();
            set.Clear();
        }

        public bool Contains(T item) => set.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => set.CopyTo(array, arrayIndex);
        public bool Remove(T item)
        {
            ThrowIfFrozen();
            return set.Remove(item);
        }
        public IEnumerator<T> GetEnumerator() => set.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => set.GetEnumerator();
    }
}
