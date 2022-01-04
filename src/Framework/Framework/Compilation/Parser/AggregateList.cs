using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser
{
    public class AggregateList<T> : IReadOnlyList<T>
    {
        Part firstPart;
        List<Part>? parts;

        public void Add(IReadOnlyList<T> list, int len = -1, int from = 0) => Add(new Part(list, from, len < 0 ? list.Count : len));

        public void Add(Part p)
        {
            if (p.len == 0) return;
            if (firstPart.len == 0)
            {
                firstPart = p;
                return;
            }
            var last = (parts == null ? firstPart : parts[parts.Count - 1]);
            if (last.list == p.list && last.from + last.len == p.from)
            {
                if (parts == null) firstPart = firstPart.AddLen(p.len);
                else
                {
                    parts[parts.Count - 1] = last.AddLen(p.len);
                }
            }
            else
            {
                if (parts == null) parts = new List<Part>();
                parts.Add(p);
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < firstPart.len) return firstPart.list[firstPart.from + index];
                if (parts == null) throw new IndexOutOfRangeException();
                index -= firstPart.len;
                for (int i = 0; i < parts.Count; i++)
                {
                    if (parts[i].len > index) return parts[i].list[parts[i].from + index];
                    index -= parts[i].len;
                }
                throw new IndexOutOfRangeException();
            }
        }

        public int Count
        {
            get
            {
                var r = firstPart.len;
                if (parts != null)
                {
                    for (int i = 0; i < parts.Count; i++)
                    {
                        r += parts[i].len;
                    }
                }
                return r;
            }
        }

        public Part FindElement(Predicate<T> predicate)
        {
            if (firstPart.len == 0) return default(Part);
            var findex = Find(firstPart, predicate);
            if (findex >= 0) return new Part(firstPart.list, findex, 1);
            if (parts == null) return default(Part);
            foreach (var p in parts)
            {
                findex = Find(p, predicate);
                if (findex >= 0) return new Part(p.list, findex, 1);
            }
            return default(Part);
        }

        private int Find(Part list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.len; i++)
            {
                if (predicate(list.list[i + list.from]))
                {
                    return i + list.from;
                }
            }
            return -1;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(firstPart, parts);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [return: MaybeNull]
        public T FirstOrDefault() => firstPart.len > 0 ? firstPart.list[firstPart.from] : default(T)!;
        public T First() => firstPart.len > 0 ? firstPart.list[firstPart.from] : throw new InvalidOperationException("AggregateList does not contain any element");
        public T Last() => parts != null ? parts[parts.Count - 1].Last() : firstPart.Last();
        [return: MaybeNull]
        public T LastOrDefault() => parts != null ? parts[parts.Count - 1].Last() : firstPart.LastOrDefault()!;

        public bool TryGetSinglePart(out Part part)
        {
            part = firstPart;
            return parts == null || parts.Count == 0;
        }
        public bool Any() => firstPart.len > 0;

        public struct Enumerator : IEnumerator<T>
        {
            private Part firstPart;
            private List<Part>? parts;
            private int index;
            private int pindex;

            public Enumerator(Part firstPart, List<Part>? parts)
            {
                this.firstPart = firstPart;
                this.parts = parts;
                index = firstPart.from - 1;
                pindex = -1;
            }

            private Part CurrentPart => pindex < 0 ? firstPart : parts![pindex];

            public T Current => CurrentPart.list[index];

            object? IEnumerator.Current => Current;

            public void Dispose() { parts = null; }

            public bool MoveNext()
            {
                var p = CurrentPart;
                if (p.len + p.from > ++index) return true;
                if (parts != null && parts.Count > ++pindex)
                {
                    index = CurrentPart.from;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                pindex = -1;
                index = firstPart.from - 1;
            }
        }
        public struct Part : IEnumerable<T>
        {
            public readonly IReadOnlyList<T> list;
            public readonly int from;
            public readonly int len;

            public Part(IReadOnlyList<T> list, int from, int len)
            {
                this.list = list;
                this.from = from;
                this.len = len;
                if (len < 0) throw new IndexOutOfRangeException("len < 0");
                if (list.Count < from + len) throw new IndexOutOfRangeException();
            }

            public Part WithLen(int newLen) => new Part(list, from, newLen);
            public Part AddLen(int addLen) => new Part(list, from, len + addLen);

            public IEnumerator<T> GetEnumerator() => new AggregateList<T>.Enumerator(this, null);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool Any => len > 0;

            public T Last() => len > 0 ? list[from + len - 1] : throw new InvalidOperationException("AggregateList does not contain any element.");
            [return: MaybeNull]
            public T LastOrDefault() => len > 0 ? list[from + len - 1] : default(T)!;
        }
    }
}
