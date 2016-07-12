using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using System.Reflection;

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

        public bool HasAncestor<T>() where T : DotvvmControl
        {
            return HasAncestor(typeof(T));
        }

        public bool HasAncestor(Type parentType)
        {
            return Ancestors.Any(a => a.Control.Metadata.Type == parentType);
        }

        public bool HasAncestorsOrdered(IEnumerable<Type> parentTypes)
        {
            using (var enumerator = parentTypes.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return true;

                foreach (var parent in Ancestors)
                {
                    if(parent.Control.Metadata.Type == enumerator.Current)
                    {
                        if (!enumerator.MoveNext()) return true;
                    }
                }
            }
            return false;
        }

        public bool HasAncestorsOrdered(params Type[] parentTypes)
        {
            return HasAncestorsOrdered(parentTypes as IEnumerable<Type>);
        }

        public bool HasParent<T>()
            where T: DotvvmControl
        {
            return Parent != null && typeof(T).IsAssignableFrom(Parent.Control.Metadata.Type);
        }

        public bool HasProperty(DotvvmProperty property)
        {
            return Control.Properties.ContainsKey(property);
        }

        public bool HasHtmlAttribute(string attributeName)
        {
            return Control.HtmlAttributes.ContainsKey(attributeName);
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

        public Type DataContext()
        {
            return Control.DataContextTypeStack.DataContextType;
        }

        public bool HasDataContext<T>()
        {
            return typeof(T).IsAssignableFrom(Control.DataContextTypeStack.DataContextType);
        }
    }
}
