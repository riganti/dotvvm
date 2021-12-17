using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DotVVM.Framework.Configuration;

// Tree architecture is inspired by NRefactory, large pieces of code are copy-pasted, see https://github.com/icsharpcode/NRefactory for source
namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public abstract class JsNode: AbstractAnnotatable
    {
        public override string ToString() => this.Clone().FormatScript(isDebugString: true);

        private bool isFrozen;
        public bool IsFrozen => isFrozen;
        protected void ThrowIfFrozen()
        {
            if (isFrozen) throw FreezableUtils.Error(this.GetType().Name);
        }

        public void Freeze()
        {
            isFrozen = true;
            for (var child = firstChild; child != null; child = child.nextSibling)
                child.Freeze();
        }

        JsNode? firstChild;
        JsNode? lastChild;
        JsNode? parent;
        JsNode? prevSibling;
        JsNode? nextSibling;

        private JsTreeRole? role;
        public JsTreeRole? Role
        {
            get { return role; }
            set { role = value; }
        }

        public JsNode? Parent => parent;
        public JsNode? NextSibling => nextSibling;
        public JsNode? PrevSibling => prevSibling;
        public JsNode? FirstChild => firstChild;
        public JsNode? LastChild => lastChild;
        public bool HasChildren => firstChild != null;

        public ChildrenCollection Children => new ChildrenCollection(this);
        // {
        //     get {
        //         JsNode? next;
        //         for (var cur = firstChild; cur != null; cur = next) {
        //             Debug.Assert(cur.parent == this);
        //             // Remember next before yielding cur.
        //             // This allows removing/replacing nodes while iterating through the list.
        //             next = cur.nextSibling;
        //             yield return cur;
        //         }
        //     }
        // }

        public struct ChildrenCollection : IEnumerable<JsNode>
        {
            JsNode node;
            public ChildrenCollection(JsNode node)
            {
                this.node = node;
            }

            public bool Any() => node.firstChild is not null;

            public ChildrenEnumerator GetEnumerator() => new ChildrenEnumerator(node.firstChild);
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<JsNode> IEnumerable<JsNode>.GetEnumerator() => throw new NotImplementedException();
        }

        public struct ChildrenEnumerator : IEnumerator<JsNode>
        {
            private JsNode? next;
            private JsNode? cur;

            public ChildrenEnumerator(JsNode? firstChild)
            {
                this.next = firstChild;
                this.cur = null;
            }

            public JsNode Current => cur!;
            public bool MoveNext()
            {
                if (next is null)
                    return false;
                cur = next;
                next = next.nextSibling;
                return true;
            }
            object System.Collections.IEnumerator.Current => cur!;
            public void Dispose() { }
            void System.Collections.IEnumerator.Reset() { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the ancestors of this node (excluding this node itself)
        /// </summary>
        public IEnumerable<JsNode> Ancestors
        {
            get {
                for (JsNode? cur = parent; cur != null; cur = cur.parent) {
                    yield return cur;
                }
            }
        }

        /// <summary>
        /// Gets the ancestors of this node (including this node itself)
        /// </summary>
        public IEnumerable<JsNode> AncestorsAndSelf
        {
            get {
                for (JsNode? cur = this; cur != null; cur = cur.parent) {
                    yield return cur;
                }
            }
        }


        /// <summary>
        /// Gets all descendants of this node (excluding this node itself) in pre-order.
        /// </summary>
        public IEnumerable<JsNode> Descendants => GetDescendantsImpl(false);

        /// <summary>
        /// Gets all descendants of this node (including this node itself) in pre-order.
        /// </summary>
        public IEnumerable<JsNode> DescendantsAndSelf => GetDescendantsImpl(true);

        public IEnumerable<JsNode> DescendantNodes(Func<JsNode, bool>? descendIntoChildren = null)
        {
            return GetDescendantsImpl(false, descendIntoChildren);
        }

        public IEnumerable<JsNode> DescendantNodesAndSelf(Func<JsNode, bool>? descendIntoChildren = null)
        {
            return GetDescendantsImpl(true, descendIntoChildren);
        }

        IEnumerable<JsNode> GetDescendantsImpl(bool includeSelf, Func<JsNode, bool>? descendIntoChildren = null)
        {
            if (includeSelf) {
                yield return this;
                if (descendIntoChildren != null && !descendIntoChildren(this))
                    yield break;
            }

            var nextStack = new Stack<JsNode?>();
            var pos = firstChild;
            while (pos != null) {
                // Remember next before yielding pos.
                // This allows removing/replacing nodes while iterating through the list.
                var sibling = pos.nextSibling;
                yield return pos;
                if (pos.firstChild != null && (descendIntoChildren == null || descendIntoChildren(pos)))
                {
                    if (sibling != null)
                        nextStack.Push(sibling);
                    pos = pos.firstChild;
                }
                else if (sibling != null)
                    pos = sibling;
                else
                    nextStack.TryPop(out pos);
            }
        }

        /// <summary>
		/// Gets the first child with the specified role.
		/// Returns the role's null object if the child is not found.
		/// </summary>
		public T? GetChildByRole<T>(JsTreeRole<T> role) where T : JsNode
        {
            if (role == null)
                throw new ArgumentNullException("role");
            for (var cur = firstChild; cur != null; cur = cur.nextSibling) {
                if (cur.role == role)
                    return (T)cur;
            }
            return null;
        }

        public JsNodeCollection<T> GetChildrenByRole<T>(JsTreeRole<T> role) where T : JsNode
        {
            return new JsNodeCollection<T>(this, role);
        }

        protected void SetChildByRole<T>(JsTreeRole<T> role, T? newChild) where T : JsNode
        {
            if (GetChildByRole(role) is T oldChild)
                oldChild.ReplaceWith(newChild);
            else
                AddChild(newChild, role);
        }

        public void AddChild<T>(T? child, JsTreeRole<T> role) where T : JsNode
        {
            if (role == null) throw new ArgumentNullException("role");
            if (child == null)
                return;
            ThrowIfFrozen();
            if (child == this) throw new ArgumentException("Cannot add a node to itself as a child.", "child");
            if (child.parent != null) throw new ArgumentException("Node is already used in another tree.", "child");
            if (child.IsFrozen) throw new ArgumentException("Cannot add a frozen node.", "child");
            AddChildUnsafe(child, role);
        }

        public void AddChildWithExistingRole(JsNode? child)
        {
            if (child == null)
                return;
            ThrowIfFrozen();
            if (child == this)
                throw new ArgumentException("Cannot add a node to itself as a child.", "child");
            if (child.parent != null)
                throw new ArgumentException("Node is already used in another tree.", "child");
            if (child.IsFrozen)
                throw new ArgumentException("Cannot add a frozen node.", "child");
            AddChildUnsafe(child, child.Role);
        }

        /// <summary>
        /// Adds a child without performing any safety checks.
        /// </summary>
        internal void AddChildUnsafe(JsNode child, JsTreeRole? role)
        {
            child.parent = this;
            child.role = role;
            if (firstChild == null) {
                lastChild = firstChild = child;
            } else {
                lastChild!.nextSibling = child;
                child.prevSibling = lastChild;
                lastChild = child;
            }
        }

        public void InsertChildBefore<T>(JsNode? nextSibling, T child, JsTreeRole<T> role) where T : JsNode
        {
            if (role == null)
                throw new ArgumentNullException("role");
            if (nextSibling == null) {
                AddChild(child, role);
                return;
            }

            if (child == null)
                return;
            ThrowIfFrozen();
            if (child.parent != null)
                throw new ArgumentException("Node is already used in another tree.", "child");
            if (child.IsFrozen)
                throw new ArgumentException("Cannot add a frozen node.", "child");
            if (nextSibling.parent != this)
                throw new ArgumentException("NextSibling is not a child of this node.", "nextSibling");
            // No need to test for "Cannot add children to null nodes",
            // as there isn't any valid nextSibling in null nodes.
            InsertChildBeforeUnsafe(nextSibling, child, role);
        }

        internal void InsertChildBeforeUnsafe(JsNode nextSibling, JsNode child, JsTreeRole role)
        {
            child.parent = this;
            child.role = role;
            child.nextSibling = nextSibling;
            child.prevSibling = nextSibling.prevSibling;

            if (nextSibling.prevSibling != null) {
                Debug.Assert(nextSibling.prevSibling.nextSibling == nextSibling);
                nextSibling.prevSibling.nextSibling = child;
            } else {
                Debug.Assert(firstChild == nextSibling);
                firstChild = child;
            }
            nextSibling.prevSibling = child;
        }

        public void InsertChildAfter<T>(JsNode? prevSibling, T child, JsTreeRole<T> role) where T : JsNode
        {
            InsertChildBefore(prevSibling == null ? firstChild : prevSibling.nextSibling, child, role);
        }

        /// <summary>
        /// Removes this node from its parent.
        /// </summary>
        public JsNode Remove()
        {
            if (parent != null) {
                ThrowIfFrozen();
                if (prevSibling != null) {
                    Debug.Assert(prevSibling.nextSibling == this);
                    prevSibling.nextSibling = nextSibling;
                } else {
                    Debug.Assert(parent.firstChild == this);
                    parent.firstChild = nextSibling;
                }
                if (nextSibling != null) {
                    Debug.Assert(nextSibling.prevSibling == this);
                    nextSibling.prevSibling = prevSibling;
                } else {
                    Debug.Assert(parent.lastChild == this);
                    parent.lastChild = prevSibling;
                }
                parent = null;
                prevSibling = null;
                nextSibling = null;
            }
            return this;
        }

        /// <summary>
        /// Replaces this node with the new node.
        /// </summary>
        public void ReplaceWith(JsNode? newNode)
        {
            if (newNode == null) {
                Remove();
                return;
            }
            if (newNode == this) return; // nothing to do...
            if (parent == null) {
                throw new InvalidOperationException("Cannot replace the root node");
            }
            ThrowIfFrozen();
            // Because this method doesn't statically check the new node's type with the role,
            // we perform a runtime test:
            if (!this.Role!.IsValid(newNode)) {
                throw new ArgumentException($"The new node '{newNode.GetType().Name}' is not valid in the role {this.Role.ToString()}", "newNode");
            }
            if (newNode.parent != null) {
                // newNode is used within this tree?
                if (newNode.Ancestors.Contains(this)) {
                    // e.g. "parenthesizedExpr.ReplaceWith(parenthesizedExpr.Expression);"
                    // enable automatic removal
                    newNode.Remove();
                } else {
                    throw new ArgumentException("Node is already used in another tree.", "newNode");
                }
            }
            if (newNode.IsFrozen)
                throw new ArgumentException("Cannot add a frozen node.", "newNode");

            newNode.parent = parent;
            newNode.role = this.Role;
            newNode.prevSibling = prevSibling;
            newNode.nextSibling = nextSibling;

            if (prevSibling != null) {
                Debug.Assert(prevSibling.nextSibling == this);
                prevSibling.nextSibling = newNode;
            } else {
                Debug.Assert(parent.firstChild == this);
                parent.firstChild = newNode;
            }
            if (nextSibling != null) {
                Debug.Assert(nextSibling.prevSibling == this);
                nextSibling.prevSibling = newNode;
            } else {
                Debug.Assert(parent.lastChild == this);
                parent.lastChild = newNode;
            }
            parent = null;
            prevSibling = null;
            nextSibling = null;
        }

        internal JsNode CloneImpl()
        {
            JsNode copy = (JsNode)this.MemberwiseClone();
            // First, reset the shallow pointer copies
            copy.parent = null;
            copy.firstChild = null;
            copy.lastChild = null;
            copy.prevSibling = null;
            copy.nextSibling = null;
            copy.isFrozen = false;

            // Then perform a deep copy:
            for (JsNode? cur = firstChild; cur != null; cur = cur.nextSibling) {
                copy.AddChildUnsafe(cur.CloneImpl(), cur.Role);
            }

            // Finally, clone the annotation, if necessary
            copy.CloneAnnotations();

            return copy;
        }

        public abstract void AcceptVisitor(IJsNodeVisitor visitor);

        public JsNode? GetNextNode()
        {
            if (NextSibling != null)
                return NextSibling;
            if (Parent != null)
                return Parent.GetNextNode();
            return null;
        }

        /// <summary>
        /// Gets the next node which fulfills a given predicate
        /// </summary>
        /// <returns>The next node.</returns>
        /// <param name="pred">The predicate.</param>
        public JsNode? GetNextNode(Func<JsNode, bool> pred)
        {
            var next = GetNextNode();
            while (next != null && !pred(next))
                next = next.GetNextNode();
            return next;
        }

        public JsNode? GetPrevNode()
        {
            if (PrevSibling != null)
                return PrevSibling;
            if (Parent != null)
                return Parent.GetPrevNode();
            return null;
        }

        /// <summary>
        /// Gets the previous node which fulfills a given predicate
        /// </summary>
        /// <returns>The next node.</returns>
        /// <param name="pred">The predicate.</param>
        public JsNode? GetPrevNode(Func<JsNode, bool> pred)
        {
            var prev = GetPrevNode();
            while (prev != null && !pred(prev))
                prev = prev.GetPrevNode();
            return prev;
        }

        /// <summary>
        /// Gets the next sibling which fulfills a given predicate
        /// </summary>
        /// <returns>The next node.</returns>
        /// <param name="pred">The predicate.</param>
        public JsNode? GetNextSibling(Func<JsNode, bool> pred)
        {
            var next = NextSibling;
            while (next != null && !pred(next))
                next = next.NextSibling;
            return next;
        }

        /// <summary>
        /// Gets the next sibling which fulfills a given predicate
        /// </summary>
        /// <returns>The next node.</returns>
        /// <param name="pred">The predicate.</param>
        public JsNode? GetPrevSibling(Func<JsNode, bool> pred)
        {
            var prev = PrevSibling;
            while (prev != null && !pred(prev))
                prev = prev.PrevSibling;
            return prev;
        }
    }
}
