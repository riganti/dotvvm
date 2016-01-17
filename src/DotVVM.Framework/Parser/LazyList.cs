//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DotVVM.Framework.Parser
//{
//    public struct LazyList<T> : IReadOnlyList<T>
//    {
//        public List<T> list;

//        public T this[int index]
//        {
//            get
//            {
//                if (list == null) throw new IndexOutOfRangeException();
//                return list[index];
//            }
//            set
//            {
//                if (list == null) throw new IndexOutOfRangeException();
//                list[index] = value;
//            }
//        }

//        public int Count => list == null ? 0 : list.Count;

//        public bool IsReadOnly => false;

//        public void Clear()
//        {
//            if (list != null) list.Clear();
//        }

//        public bool Contains(T item)
//        {
//            if (list == null) return false;
//            return list.Contains(item);
//        }

//        public void CopyTo(T[] array, int arrayIndex)
//        {
//            if (list != null) list.CopyTo(array, arrayIndex);
//        }

//        public IEnumerator<T> GetEnumerator()
//        {
//            if (list == null) return Enumerable.Empty<T>().GetEnumerator();
//            return list.GetEnumerator();
//        }

//        public List<T> GetList() => list;

//        public int IndexOf(T item)
//        {
//            if (list == null) return -1;
//            return list.IndexOf(item);
//        }

//        public bool Remove(T item)
//        {
//            if (list != null) return list.Remove(item);
//            else return false;
//        }

//        public void RemoveAt(int index)
//        {
//            if (list == null) throw new IndexOutOfRangeException();
//            list.RemoveAt(index);
//        }

//        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

//    }
//    public static class LazyList
//    {
//        public static void Add<T>(ref LazyList<T> list, T item)
//        {
//            if (list.list == null) list.list = new List<T>();
//            list.list.Add(item);
//        }
//    }
//}
