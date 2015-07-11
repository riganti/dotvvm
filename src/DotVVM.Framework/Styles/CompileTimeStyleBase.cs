using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Styles
{
    public abstract class CompileTimeStyleBase : IStyle
    {
        public Dictionary<string, object> SetHtmlAttributes { get; set; }
        public Dictionary<DotvvmProperty, ResolvedPropertySetter> SetProperties { get; set; }

        public IStyleApplicator Applicator => new StyleApplicator(this);

        public abstract Type ControlType { get; }

        public bool ExactTypeMatch { get; protected set; }

        public abstract bool Matches(StyleMatchingInfo matcher);

        protected virtual void ApplyStyle(ResolvedControl control)
        {
            if(SetHtmlAttributes != null)
            {
                foreach (var attr in SetHtmlAttributes)
                {
                    if(!control.HtmlAttributes.ContainsKey(attr.Key))
                    {
                        control.HtmlAttributes.Add(attr.Key, attr.Value);
                    }
                }
            }
            if(SetProperties != null)
            {
                foreach (var prop in SetProperties)
                {
                    if(!control.Properties.ContainsKey(prop.Key))
                    {
                        control.Properties.Add(prop.Key, prop.Value);
                    }
                }
            }
        }

        class StyleApplicator : IStyleApplicator
        {
            CompileTimeStyleBase @this;

            public StyleApplicator(CompileTimeStyleBase @this)
            {
                this.@this = @this;
            }

            public void ApplyStyle(ResolvedControl control)
            {
                @this.ApplyStyle(control);
            }
        }
    }
}
