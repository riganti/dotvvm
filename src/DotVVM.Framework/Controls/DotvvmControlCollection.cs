using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Resources;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Contains child controls of a <see cref="DotvvmControl"/>.
    /// </summary>
    public class DotvvmControlCollection : IList<DotvvmControl>
    {

        private DotvvmControl parent;
        private List<DotvvmControl> controls = new List<DotvvmControl>();


        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmControlCollection"/> class.
        /// </summary>
        public DotvvmControlCollection(DotvvmControl parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<DotvvmControl> GetEnumerator()
        {
            return controls.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public void Add(DotvvmControl item)
        {
            Insert(controls.Count, item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            foreach (var item in controls)
            {
                item.Parent = null;
            }
            controls.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(DotvvmControl item)
        {
            return item.Parent == parent && controls.Contains(item);
        }

        /// <summary>
        /// Copies the controls to the specified array.
        /// </summary>
        public void CopyTo(DotvvmControl[] array, int arrayIndex)
        {
            controls.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public bool Remove(DotvvmControl item)
        {
            if (item.Parent == parent)
            {
                item.Parent = null;
                controls.Remove(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count
        {
            get { return controls.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>
        /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(DotvvmControl item)
        {
            return controls.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public void Insert(int index, DotvvmControl item)
        {
            SetParent(item);
            controls.Insert(index, item);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            var item = controls[index];
            item.Parent = null;
            controls.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public DotvvmControl this[int index]
        {
            get { return controls[index]; }
            set
            {
                controls[index].Parent = null;
                SetParent(value);
                controls[index] = value;
            }
        }

        /// <summary>
        /// Sets the parent to the specified control.
        /// </summary>
        private void SetParent(DotvvmControl item)
        {
            if (item.Parent != null)
            {
                throw new InvalidOperationException(Parser_Dothtml.ControlCollection_ControlAlreadyHasParent);
            }
            item.Parent = parent;
        }
    }
}