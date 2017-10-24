using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using System.Reflection;
using System.IO;

namespace DotVVM.Framework.Compilation.Styles
{
    public class StyleMatchContext
    {
        public StyleMatchContext Parent { get; set; }
        public ResolvedControl Control { get; set; }

        public IEnumerable<StyleMatchContext> Ancestors
        {
            get
            {
                var c = Parent;
                while (c != null)
                {
                    yield return c;
                    c = c.Parent;
                }
            }
        }

        public IEnumerable<StyleMatchContext> AncestorsOfType<T>()
        {
            return Ancestors.Where(a => typeof(T).IsAssignableFrom(a.Control.Metadata.Type));
        }

        /// <summary>
        /// Determines whether the control has an ancestor of the given type.
        /// </summary>
        public bool HasAncestor<T>() where T : DotvvmControl
        {
            return HasAncestor(typeof(T));
        }

        /// <summary>
        /// Determines whether the control has an ancestor of the <paramref name="parentType"/> type.
        /// </summary>
        public bool HasAncestor(Type parentType)
        {
            return Ancestors.Any(a => a.Control.Metadata.Type == parentType);
        }

        /// <summary>
        /// Determines whether the control's ancestors types correspond to those in <paramref name="parentTypes"/>.
        /// </summary>
        public bool HasAncestorsOrdered(IEnumerable<Type> parentTypes)
        {
            using (var enumerator = parentTypes.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return true;

                foreach (var parent in Ancestors)
                {
                    if (parent.Control.Metadata.Type == enumerator.Current)
                    {
                        if (!enumerator.MoveNext()) return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the control's ancestors types correspond to those given.
        /// </summary>
        public bool HasAncestorsOrdered(params Type[] parentTypes)
        {
            return HasAncestorsOrdered(parentTypes as IEnumerable<Type>);
        }

        /// <summary>
        /// Determines whether the control's parent's type is <typeparamref name="T"/>.
        /// </summary>
        public bool HasParent<T>()
            where T : DotvvmControl
        {
            return Parent != null && typeof(T).IsAssignableFrom(Parent.Control.Metadata.Type);
        }

        /// <summary>
        /// Determines whether the control has the given <see cref="DotvvmProperty"/>.
        /// </summary>
        public bool HasProperty(DotvvmProperty property)
        {
            return Control.Properties.ContainsKey(property);
        }

        /// <summary>
        /// Determines whether the control has an HTML attribute of the specified name.
        /// </summary>
        public bool HasHtmlAttribute(string attributeName)
        {
            return HasPropertyGroupMember("", attributeName);
        }

        public bool HasPropertyGroupMember(string prefix, string memberName)
        {
            var prop = Control.Metadata.PropertyGroups.FirstOrDefault(p => p.Prefix == prefix).PropertyGroup;
            return prop != null && HasProperty((DotvvmProperty)prop.GetDotvvmProperty(memberName));
        }

        public T Property<T>(DotvvmProperty property)
            where T : class
        {
            ResolvedPropertySetter s;
            if (Control.Properties.TryGetValue(property, out s))
            {
                return (s as ResolvedPropertyValue)?.Value as T;
            }
            return null;
        }

        /// <summary>
        /// Gets the DataContext of the control.
        /// </summary>
        public Type DataContext()
        {
            return Control.DataContextTypeStack.DataContextType;
        }

        /// <summary>
        /// Determines whether the control has DataContext of the given type.
        /// </summary>
        public bool HasDataContext<T>()
        {
            return typeof(T).IsAssignableFrom(DataContext());
        }

        /// <summary>
        /// Determines whether the control is in a page with a ViewModel of the given type.
        /// </summary>
        public bool HasRootDataContext<T>()
        {
            var current = Control.DataContextTypeStack;
            while (current.Parent != null)
            {
                current = current.Parent;
            }

            return typeof(T).IsAssignableFrom(current.DataContextType);
        }

        /// <summary>
        /// Determines whether the control is in a page whose View is in the given directory.
        /// </summary>
        public bool HasViewInDirectory(string directoryPath)
        {
            if (directoryPath.StartsWith("~/", StringComparison.Ordinal))
            {
                directoryPath = directoryPath.Substring(2);
            }

            return Control.TreeRoot.FileName.StartsWith(directoryPath);
        }
    }
}
