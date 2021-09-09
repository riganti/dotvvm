using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    // Following code is taken from NRefactory (https://github.com/icsharpcode/NRefactory/blob/master/ICSharpCode.NRefactory/IAnnotatable.cs)

    /// <summary>
    /// Provides an interface to handle annotations in an object.
    /// </summary>
    public interface IAnnotatable
    {
        /// <summary>
        /// Gets all annotations stored on this IAnnotatable.
        /// </summary>
        IEnumerable<object> Annotations { get; }

        /// <summary>
        /// Gets the first annotation of the specified type.
        /// Returns null if no matching annotation exists.
        /// </summary>
        /// <typeparam name='T'>
        /// The type of the annotation.
        /// </typeparam>
        T? Annotation<T>() where T : class;

        /// <summary>
        /// Gets the first annotation of the specified type.
        /// Returns null if no matching annotation exists.
        /// </summary>
        /// <param name='type'>
        /// The type of the annotation.
        /// </param>
        object? Annotation(Type type);

        /// <summary>
        /// Adds an annotation to this instance.
        /// </summary>
        /// <param name='annotation'>
        /// The annotation to add.
        /// </param>
        T AddAnnotation<T>(T annotation);

        /// <summary>
        /// Removes all annotations of the specified type.
        /// </summary>
        /// <typeparam name='T'>
        /// The type of the annotations to remove.
        /// </typeparam>
        void RemoveAnnotations<T>() where T : class;

        /// <summary>
        /// Removes all annotations of the specified type.
        /// </summary>
        /// <param name='type'>
        /// The type of the annotations to remove.
        /// </param>
        void RemoveAnnotations(Type type);
    }

    /// <summary>
    /// Base class used to implement the IAnnotatable interface.
    /// This implementation is thread-safe.
    /// </summary>
    public abstract class AbstractAnnotatable : IAnnotatable
    {
        // Annotations: points either null (no annotations), to the single annotation,
        // or to an AnnotationList.
        // Once it is pointed at an AnnotationList, it will never change (this allows thread-safety support by locking the list)

        object? annotations;

        /// <summary>
        /// Clones all annotations.
        /// This method is intended to be called by Clone() implementations in derived classes.
        /// <code>
        /// var copy = (AstNode)MemberwiseClone();
        /// copy.CloneAnnotations();
        /// </code>
        /// </summary>
        protected void CloneAnnotations()
        {
            if (annotations is AnnotationList cloneable)
                annotations = cloneable.Clone();
        }

        sealed class AnnotationList : List<object>
        {
            // There are two uses for this custom list type:
            // 1) it's private, and thus (unlike List<object>) cannot be confused with real annotations
            // 2) It allows us to simplify the cloning logic by making the list behave the same as a cloneable annotation.
            public AnnotationList(int initialCapacity) : base(initialCapacity)
            {
            }

            public object Clone()
            {
                lock (this)
                {
                    AnnotationList copy = new AnnotationList(this.Count);
                    copy.AddRange(this);
                    return copy;
                }
            }
        }

        public virtual T AddAnnotation<T>(T annotation)
        {
            if (annotation == null)
                throw new ArgumentNullException("annotation");
            retry: // Retry until successful
            object oldAnnotation = Interlocked.CompareExchange(ref this.annotations, annotation, null);
            if (oldAnnotation == null)
            {
                return annotation; // we successfully added a single annotation
            }
            AnnotationList? list = oldAnnotation as AnnotationList;
            if (list == null)
            {
                // we need to transform the old annotation into a list
                list = new AnnotationList(4);
                list.Add(oldAnnotation);
                list.Add(annotation);
                if (Interlocked.CompareExchange(ref this.annotations, list, oldAnnotation) != oldAnnotation)
                {
                    // the transformation failed (some other thread wrote to this.annotations first)
                    goto retry;
                }
            }
            else
            {
                // once there's a list, use simple locking
                lock (list)
                {
                    list.Add(annotation);
                }
            }
            return annotation;
        }

        public virtual void RemoveAnnotations<T>() where T : class
        {
            retry: // Retry until successful
            var oldAnnotations = this.annotations;
            if (oldAnnotations is AnnotationList list)
            {
                lock (list)
                    list.RemoveAll(obj => obj is T);
            }
            else if (oldAnnotations is T)
            {
                if (Interlocked.CompareExchange(ref this.annotations, null, oldAnnotations) != oldAnnotations)
                {
                    // Operation failed (some other thread wrote to this.annotations first)
                    goto retry;
                }
            }
        }

        public virtual void RemoveAnnotations(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            retry: // Retry until successful
            var oldAnnotations = this.annotations;
            if (oldAnnotations is AnnotationList list)
            {
                lock (list)
                    list.RemoveAll(type.IsInstanceOfType);
            }
            else if (type.IsInstanceOfType(oldAnnotations))
            {
                if (Interlocked.CompareExchange(ref this.annotations, null, oldAnnotations) != oldAnnotations)
                {
                    // Operation failed (some other thread wrote to this.annotations first)
                    goto retry;
                }
            }
        }

        public T? Annotation<T>() where T : class
        {
            var annotations = this.annotations;
            if (annotations is AnnotationList list)
            {
                lock (list)
                {
                    foreach (object obj in list)
                    {
                        if (obj is T t)
                            return t;
                    }
                    return null;
                }
            }
            else
            {
                return annotations as T;
            }
        }

        public object? Annotation(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            var annotations = this.annotations;
            if (annotations is AnnotationList list)
            {
                lock (list)
                {
                    foreach (object obj in list)
                    {
                        if (type.IsInstanceOfType(obj))
                            return obj;
                    }
                }
            }
            else
            {
                if (type.IsInstanceOfType(annotations))
                    return annotations;
            }
            return null;
        }

        /// <summary>
        /// Gets all annotations stored on this node.
        /// </summary>
        public IEnumerable<object> Annotations
        {
            get
            {
                var annotations = this.annotations;
                if (annotations is AnnotationList list)
                {
                    lock (list)
                    {
                        return list.ToArray();
                    }
                }
                else
                {
                    if (annotations != null)
                        return new object[] { annotations };
                    else
                        return Enumerable.Empty<object>();
                }
            }
        }
    }

    public static class AnnotatableUtils
    {
        public static T WithAnnotation<T>(this T node, object? annotation, bool append = true)
            where T : class, IAnnotatable
        {
            if (annotation != null && (append || !node.HasAnnotation<T>())) node.AddAnnotation(annotation);
            return node;
        }

        public static TNode WithoutAnnotation<TAnnotation, TNode>(this TNode node)
            where TNode : IAnnotatable
            where TAnnotation : class
        {
            node.RemoveAnnotations<TAnnotation>();
            return node;
        }

        public static T WithAnnotations<T>(this T node, IEnumerable<object?> annotations)
            where T : class, IAnnotatable
        {
            foreach (var annotation in annotations) if (annotation != null) node.AddAnnotation(annotation);
            return node;
        }

        public static bool HasAnnotation<T>(this IAnnotatable node)
            where T : class
            => node.Annotation<T>() != null;

        public static bool TryGetAnnotation<T>(this IAnnotatable node, [NotNullWhen(true)] out T? annotation)
            where T : class
            => (annotation = node.Annotation<T>()) != null;

    }
}
