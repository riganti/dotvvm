using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
	/// <summary>
	/// Represents the children of an JsNode that have a specific role.
	/// </summary>
	public class JsNodeCollection<T> : ICollection<T>
		#if NET_4_5
		, IReadOnlyCollection<T>
		#endif
		where T : JsNode
	{
		readonly JsNode node;
		readonly JsTreeRole<T> role;
		
		public JsNodeCollection(JsNode node, JsTreeRole<T> role)
		{
			if (node == null)
				throw new ArgumentNullException("node");
			if (role == null)
				throw new ArgumentNullException("role");
			this.node = node;
			this.role = role;
		}
		
		public int Count {
			get {
				int count = 0;
				for (var cur = node.FirstChild; cur != null; cur = cur.NextSibling) {
					if (cur.Role == role)
						count++;
				}
				return count;
			}
		}
		
		public void Add(T? element)
		{
			node.AddChild(element, role);
		}
		
		public void AddRange(IEnumerable<T?> nodes)
		{
			// Evaluate 'nodes' first, since it might change when we add the new children
			// Example: collection.AddRange(collection);
			if (nodes != null) {
				foreach (T? node in nodes.ToList())
					Add(node);
			}
		}
		
		public void AddRange(T?[] nodes)
		{
			// Fast overload for arrays - we don't need to create a copy
			if (nodes != null) {
				foreach (T? node in nodes)
					Add(node);
			}
		}
		
		public void ReplaceWith(IEnumerable<T?>? nodes)
		{
			// Evaluate 'nodes' first, since it might change when we call Clear()
			// Example: collection.ReplaceWith(collection);
			if (nodes != null)
				nodes = nodes.ToList();
			Clear();
			if (nodes != null) {
				foreach (T? node in nodes)
					Add(node);
			}
		}
		
		public void MoveTo(ICollection<T> targetCollection)
		{
			if (targetCollection == null)
				throw new ArgumentNullException("targetCollection");
			foreach (T node in this) {
				node.Remove();
				targetCollection.Add(node);
			}
		}
		
		public bool Contains([NotNullWhen(true)] T? element)
		{
			return element != null && element.Parent == node && element.Role == role;
		}
		
		public bool Remove(T? element)
		{
			if (Contains(element)) {
				element.Remove();
				return true;
			} else {
				return false;
			}
		}
		
		public void CopyTo(T[] array, int arrayIndex)
		{
			foreach (T item in this)
				array[arrayIndex++] = item;
		}
		
		public void Clear()
		{
			foreach (T item in this)
				item.Remove();
		}
		
		/// <summary>
		/// Returns the first element for which the predicate returns true,
		/// or the null node (JsNode with IsNull=true) if no such object is found.
		/// </summary>
		public T? FirstOrNullObject(Func<T, bool>? predicate = null)
		{
			foreach (T item in this)
				if (predicate == null || predicate(item))
					return item;
			return null;
		}
		
		/// <summary>
		/// Returns the last element for which the predicate returns true,
		/// or the null node (JsNode with IsNull=true) if no such object is found.
		/// </summary>
		public T? LastOrNullObject(Func<T, bool>? predicate = null)
		{
			T? result = null;
			foreach (T item in this)
				if (predicate == null || predicate(item))
					result = item;
			return result;
		}
		
		bool ICollection<T>.IsReadOnly {
			get { return false; }
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			JsNode? next;
			for (var cur = node.FirstChild; cur != null; cur = next) {
				Debug.Assert(cur.Parent == node);
				// Remember next before yielding cur.
				// This allows removing/replacing nodes while iterating through the list.
				next = cur.NextSibling;
				if (cur.Role == role)
					yield return (T)cur;
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		#region Equals and GetHashCode implementation
		public override int GetHashCode()
		{
			return node.GetHashCode() ^ role.GetHashCode();
		}
		
		public override bool Equals(object? obj)
		{
			var other = obj as JsNodeCollection<T>;
			if (other == null)
				return false;
			return this.node == other.node && this.role == other.role;
		}
		#endregion
		
		public void InsertAfter(T? existingItem, T newItem)
		{
			node.InsertChildAfter(existingItem, newItem, role);
		}
		
		public void InsertBefore(T? existingItem, T newItem)
		{
			node.InsertChildBefore(existingItem, newItem, role);
		}
		
		/// <summary>
		/// Applies the <paramref name="visitor"/> to all nodes in this collection.
		/// </summary>
		public void AcceptVisitor(IJsNodeVisitor visitor)
		{
			JsNode? next;
			for (var cur = node.FirstChild; cur != null; cur = next) {
				Debug.Assert(cur.Parent == node);
				// Remember next before yielding cur.
				// This allows removing/replacing nodes while iterating through the list.
				next = cur.NextSibling;
				if (cur.Role == role)
					cur.AcceptVisitor(visitor);
			}
		}
	}
}
