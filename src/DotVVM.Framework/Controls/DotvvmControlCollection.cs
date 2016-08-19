using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Resources;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Contains child controls of a <see cref="DotvvmControl"/>.
    /// </summary>
    public class DotvvmControlCollection : IList<DotvvmControl>
    {
        private DotvvmControl parent;
        private List<DotvvmControl> controls = new List<DotvvmControl>();

        private LifeCycleEventType lastLifeCycleEvent;


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

            item.Children.InvokeMissedPageLifeCycleEvents(lastLifeCycleEvent, isMissingInvoke: true);
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

                controls[index].Children.InvokeMissedPageLifeCycleEvents(lastLifeCycleEvent, isMissingInvoke: true);
            }
        }

        /// <summary>
        /// Sets the parent to the specified control.
        /// </summary>
        private void SetParent(DotvvmControl item)
        {
            if (item.Parent != null && item.Parent != parent)
            {
                throw new DotvvmControlException(parent, "The control cannot be added to the collection because it already has a different parent! Remove it from the original collection first.");
            }
            item.Parent = parent;
			var setrq = parent;
			while (setrq != null && (item.LifecycleRequirements & ~setrq.LifecycleRequirements) != ControlLifecycleRequirements.None)
			{
				setrq.LifecycleRequirements |= item.LifecycleRequirements;
				setrq = setrq.Parent;
			}
            if (item.GetValue(Internal.UniqueIDProperty) == null)
            {
                item.SetValue(Internal.UniqueIDProperty, parent.GetValue(Internal.UniqueIDProperty) + "a" + Count);
            }
        }

        /// <summary>
        /// Invokes missed page life cycle events on the control.
        /// </summary>
        private void InvokeMissedPageLifeCycleEvents(LifeCycleEventType targetEventType, bool isMissingInvoke)
        {
            var context = (IDotvvmRequestContext)parent.GetValue(Internal.RequestContextProperty);
            DotvvmControl lastProcessedControl = parent;
            try
            {
                InvokeMissedPageLifeCycleEvent(context, targetEventType, isMissingInvoke, ref lastProcessedControl);
            }
            catch (DotvvmInterruptRequestExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DotvvmControlException(lastProcessedControl, "Unhandled exception occured while executing page lifecycle event.", ex);
            }
        }

        private void InvokeMissedPageLifeCycleEvent(IDotvvmRequestContext context, LifeCycleEventType targetEventType, bool isMissingInvoke, ref DotvvmControl lastProcessedControl)
        {
            for (var eventType = lastLifeCycleEvent + 1; eventType <= targetEventType; eventType++)
            {
				var reqflag = (1 << ((int)eventType - 1));
				if (isMissingInvoke) reqflag = reqflag << 5;
				if ((parent.LifecycleRequirements & (ControlLifecycleRequirements)reqflag) == 0) continue;
                lastProcessedControl = parent;
                switch (eventType)
                {
                    case LifeCycleEventType.PreInit:
                        parent.OnPreInit(context);
                        break;
                    case LifeCycleEventType.Init:
                        parent.OnInit(context);
                        break;
                    case LifeCycleEventType.Load:
                        parent.OnLoad(context);
                        break;
                    case LifeCycleEventType.PreRender:
                        parent.OnPreRender(context);
                        break;
                    case LifeCycleEventType.PreRenderComplete:
                        parent.OnPreRenderComplete(context);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }

                lastLifeCycleEvent = eventType;

                foreach (var child in controls)
                {
                    child.Children.InvokeMissedPageLifeCycleEvent(context, eventType, isMissingInvoke && eventType == targetEventType, ref lastProcessedControl);
                }
            }

        }


        private void SetLastLifeCycleEvent(LifeCycleEventType eventType)
        {
            if (lastLifeCycleEvent < eventType)
            {
                lastLifeCycleEvent = eventType;
            }
        }


        /// <summary>
        /// Invokes the specified method on all controls in the page control tree.
        /// </summary>
        internal static void InvokePageLifeCycleEventRecursive(DotvvmControl rootControl, LifeCycleEventType eventType)
        {
            rootControl.Children.InvokeMissedPageLifeCycleEvents(eventType, isMissingInvoke: false);
        }
    }
}