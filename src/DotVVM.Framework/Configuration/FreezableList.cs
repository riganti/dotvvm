#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace DotVVM.Framework.Configuration
{

    static class FreezableList
    {
        public static void Freeze<T>([AllowNull] ref IList<T> list)
        {
            if (list is FreezableList<T> freezable)
                freezable.Freeze();
            else if (list is object && !list.IsReadOnly)
                list = new FreezableList<T>(list, frozen: true);
        }
    }
    sealed class FreezableList<T> : IList<T>, IReadOnlyList<T>
    {
        private readonly List<T> list;
        private bool isFrozen;
        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error("list");
        }
        public void Freeze()
        {
            this.isFrozen = true;
        }

        public FreezableList(bool frozen = false)
        {
            list = new List<T>();
            isFrozen = frozen;
        }

        public FreezableList(IEnumerable<T> items, bool frozen = false)
        {
            list = items.ToList();
            isFrozen = frozen;
        }
        public T this[int index]
        {
            get => list[index];
            set { ThrowIfFrozen(); list[index] = value; }
        }

        public int Count => list.Count;

        public bool IsReadOnly => isFrozen;

        public void Add(T item)
        {
            ThrowIfFrozen();
            list.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            ThrowIfFrozen();
            list.AddRange(items);
        }
        public void Clear()
        {
            ThrowIfFrozen();
            list.Clear();
        }
        public bool Contains(T item) => list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
        public int IndexOf(T item) => list.IndexOf(item);
        public void Insert(int index, T item)
        {
            ThrowIfFrozen();
            list.Insert(index, item);
        }
        public bool Remove(T item)
        {
            ThrowIfFrozen();
            return list.Remove(item);
        }
        public void RemoveAt(int index)
        {
            ThrowIfFrozen();
            list.RemoveAt(index);
        }
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
        public void CopyTo(Array array, int index) => ((ICollection)list).CopyTo(array, index);
    }
}
