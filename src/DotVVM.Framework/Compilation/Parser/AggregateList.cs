using System;
using System.Collections;
using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.Parser
{
    public class AggregateList<T> : IReadOnlyList<T>
    {
        Part firstPart;
        List<Part> parts;

        public void Add(IReadOnlyList<T> list, int len = -1, int from = 0) => Add(new Part(list, from, len < 0 ? list.Count : len ));

        public void Add(Part p)
        {
            if (p.len == 0) return;
            var last = (parts == null ? firstPart : parts[parts.Count - 1]);
            if (firstPart.len == 0) firstPart = p;
            else if (last.list == p.list && last.from + last.len == p.from) {
                if (parts == null) firstPart = firstPart.AddLen(p.len);
                else {
                    parts[parts.Count - 1] = last.AddLen(p.len);
                }
            }
            else {
                if (parts == null) parts = new List<Part>();
                parts.Add(p);
            }
        }

        public T this[int index] {
            get {
                if (index < firstPart.len) return firstPart.list[firstPart.from + index];
                if (parts == null) throw new IndexOutOfRangeException();
                index -= firstPart.len;
                for (int i = 0; i < parts.Count; i++) {
                    if (parts[i].len > index) return parts[i].list[parts[i].from + index];
                    index -= parts[i].len;
                }
                throw new IndexOutOfRangeException();
            }
        }

        public int Count {
            get {
                var r = firstPart.len;
                if(parts != null) {
                    for (int i = 0; i < parts.Count; i++) {
                        r += parts[i].len;
                    }
                }
                return r;
            }
        }

        public T First()
        {
            return firstPart.list[firstPart.from];
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

        public struct Enumerator : IEnumerator<T>
        {
            private Part firstPart;
            private List<Part> parts;
            private int index;
            private int pindex;

            public Enumerator(Part firstPart, List<Part> parts)
            {
                this.firstPart = firstPart;
                this.parts = parts;
                index = firstPart.from - 1;
                pindex = -1;
            }

            private Part CurrentPart => pindex < 0 ? firstPart : parts[pindex];

            public T Current => CurrentPart.list[index];

            object IEnumerator.Current => Current;

            public void Dispose() { parts = null; }

            public bool MoveNext()
            {
                var p = CurrentPart;
                if (p.len + p.from > ++index) return true;
                if(parts != null && parts.Count > ++pindex) {
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
        public struct Part: IEnumerable<T>
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
        }
    }
}
