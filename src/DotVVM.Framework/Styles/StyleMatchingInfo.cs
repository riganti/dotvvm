using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Styles
{
    public class StyleMatchingInfo
    {
        public IDictionary<Type, int> Parents { get; set; }
        public Stack<ResolvedControl> ParentStack { get; set; }
        public ResolvedControl Control { get; set; }

        public bool HasParent<T>() where T : DotvvmControl
        {
            return HasParent(typeof(T));
        }

        public bool HasParent(Type parentType)
        {
            return Parents.ContainsKey(parentType);
        }

        public bool HasParentsOrdered(IEnumerable<Type> parentTypes)
        {
            using (var enumerator = parentTypes.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return true;

                foreach (var parent in ParentStack)
                {
                    if(parent.Metadata.Type == enumerator.Current)
                    {
                        if (!enumerator.MoveNext()) return true;
                    }
                }
            }
            return false;
        }

        public bool HasParentsOrdered(params Type[] parentTypes)
        {
            return HasParentsOrdered(parentTypes as IEnumerable<Type>);
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
            if(Control.Properties.TryGetValue(property, out s))
            {
                return (s as ResolvedPropertyValue)?.Value as T;
            }
            return null;
        }
    }
}
