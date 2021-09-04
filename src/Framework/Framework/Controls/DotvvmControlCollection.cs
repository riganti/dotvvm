using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private bool isInvokingEvent;
        private int uniqueIdCounter = 0;

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
        /// Adds items to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="items">An enumeration of objects to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public void Add(IEnumerable<DotvvmControl> items)
        {
            foreach (var item in items)
            {
                controls.Add(item);
                SetParent(item);
            }
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
            uniqueIdCounter = 0;
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
        public int Count => controls.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

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
            controls.Insert(index, item);
            SetParent(item);
        }

        /// <summary>
        /// Inserts items to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="items" /> should be inserted.</param>
        /// <param name="items">An enumeration of objects to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public void Insert(int index, IEnumerable<DotvvmControl> items)
        {
            items = items.ToArray();
            controls.InsertRange(index, items);

            foreach (var item in items)
            {
                SetParent(item);
            }
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
                controls[index] = value;
                SetParent(value);
            }
        }

        /// <summary>
        /// Sets the parent to the specified control.
        /// </summary>
        private void SetParent(DotvvmControl item)
        {
            if (item.Parent != null && item.Parent != parent && IsInParentsChildren(item))
            {
                throw new DotvvmControlException(parent, "The control cannot be added to the collection " +
                    "because it already has a different parent! Remove it from the original collection first.");
            }
            item.Parent = parent;

            // Iterates through all parents and ORs the LifecycleRequirements
            var updatedLastEvent = lastLifeCycleEvent;
            {
                DotvvmControl? currentParent = parent;
                bool lifecycleEventUpdatingDisabled = false;
                while (currentParent != null && (item.LifecycleRequirements & ~currentParent.LifecycleRequirements) != ControlLifecycleRequirements.None)
                {
                    // set parent's Requirements to match the OR of all children
                    currentParent.LifecycleRequirements |= item.LifecycleRequirements;

                    // if the parent has greater last lifecycle event, update local
                    if (!lifecycleEventUpdatingDisabled && currentParent.Children.lastLifeCycleEvent > updatedLastEvent) updatedLastEvent = currentParent.Children.lastLifeCycleEvent;
                    // but don't update it, when the ancestor is invoking this event
                    if (!lifecycleEventUpdatingDisabled && currentParent.Children.isInvokingEvent) lifecycleEventUpdatingDisabled = true;
                    currentParent = GetClosestDotvvmControlAncestor(currentParent);
                }
                if (!lifecycleEventUpdatingDisabled && currentParent != null && currentParent.Children.lastLifeCycleEvent > updatedLastEvent) updatedLastEvent = currentParent.Children.lastLifeCycleEvent;
            }

            // update ancestor's last event
            if (updatedLastEvent > lastLifeCycleEvent)
            {
                DotvvmControl? currentParent = parent;
                while (currentParent != null &&!currentParent.Children.isInvokingEvent && currentParent.Children.lastLifeCycleEvent < updatedLastEvent)
                {
                    currentParent.Children.lastLifeCycleEvent = updatedLastEvent;
                    currentParent = GetClosestDotvvmControlAncestor(currentParent);
                }
            }

            if (!item.properties.Contains(Internal.UniqueIDProperty) && parent.properties.Contains(Internal.UniqueIDProperty))
            {
                AssignUniqueIds(item);
            }

            item.Children.InvokeMissedPageLifeCycleEvents(lastLifeCycleEvent, isMissingInvoke: true);



            ValidateParentsLifecycleEvents();
        }

        void AssignUniqueIds(DotvvmControl item)
        {
            Debug.Assert(parent.properties.Contains(Internal.UniqueIDProperty));
            Debug.Assert(!item.properties.Contains(Internal.UniqueIDProperty));

            item.SetValue(Internal.UniqueIDProperty, parent.GetValue(Internal.UniqueIDProperty) + "a" + uniqueIdCounter);
            uniqueIdCounter++;
            foreach (var c in item.Children)
            {
                if (!c.properties.Contains(Internal.UniqueIDProperty))
                    item.Children.AssignUniqueIds(c);
            }
        }

        [Conditional("DEBUG")]
        internal void ValidateParentsLifecycleEvents()
        {
            // check if all ancestors have the flags
            if (!parent.GetAllAncestors(onlyWhenInChildren: true).OfType<DotvvmControl>().All(c => (c.LifecycleRequirements & parent.LifecycleRequirements) == parent.LifecycleRequirements))
                throw new Exception("Internal bug in Lifecycle events.");
        }

        /// <summary>
        /// Invokes missed page life cycle events on the control.
        /// </summary>
        private void InvokeMissedPageLifeCycleEvents(LifeCycleEventType targetEventType, bool isMissingInvoke)
        {
            // just a quick check to save GetValue call
            if (lastLifeCycleEvent >= targetEventType || parent.LifecycleRequirements == ControlLifecycleRequirements.None) return;

            var context = (IDotvvmRequestContext?)parent.GetValue(Internal.RequestContextProperty);
            if (context == null)
                throw new DotvvmControlException(parent, "InvokeMissedPageLifeCycleEvents must be called on a control rooted in a view.");

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
                throw new DotvvmControlException(lastProcessedControl, "Unhandled exception occurred while executing page lifecycle event.", ex);
            }
        }

        private void InvokeMissedPageLifeCycleEvent(IDotvvmRequestContext context, LifeCycleEventType targetEventType, bool isMissingInvoke, ref DotvvmControl lastProcessedControl)
        {
            ValidateParentsLifecycleEvents();

            isInvokingEvent = true;
            for (var eventType = lastLifeCycleEvent + 1; eventType <= targetEventType; eventType++)
            {
                // get ControlLifecycleRequirements flag for the event
                var reqflag = (1 << ((int)eventType - 1));
                if (isMissingInvoke) reqflag = reqflag << 5;
                // abort when control does not require that
                if ((parent.LifecycleRequirements & (ControlLifecycleRequirements)reqflag) == 0)
                {
                    continue;
                }

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
            isInvokingEvent = false;
        }

        /// <summary>
        /// Invokes the specified method on all controls in the page control tree.
        /// </summary>
        public static void InvokePageLifeCycleEventRecursive(DotvvmControl rootControl, LifeCycleEventType eventType)
        {
            rootControl.Children.InvokeMissedPageLifeCycleEvents(eventType, isMissingInvoke: false);
        }

        private static DotvvmControl? GetClosestDotvvmControlAncestor(DotvvmControl control)
        {
            var currentParent = control.Parent;
            while (currentParent != null && !(currentParent is DotvvmControl))
            {
                currentParent = currentParent.Parent;
            }

            return (DotvvmControl?)currentParent;
        }

        private static bool IsInParentsChildren(DotvvmControl item)
        {
            return item.Parent is DotvvmControl control && control.Children.Contains(item);
        }

        /// <summary>
        /// Determines whether the control has only white space content.
        /// </summary>
        public bool HasOnlyWhiteSpaceContent()
        {
            if (controls.Count == 0) return true;

            foreach (var c in controls)
            {
                if (c is not Infrastructure.RawLiteral lit || !lit.IsWhitespace)
                    return false;
            }
            return true;
        }
    }
}
