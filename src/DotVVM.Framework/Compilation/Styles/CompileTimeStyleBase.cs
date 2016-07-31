using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation.Styles
{
    public abstract class CompileTimeStyleBase : IStyle
    {
        internal Dictionary<string, HtmlAttributeInsertionInfo> SetHtmlAttributes { get; set; }
        internal Dictionary<DotvvmProperty, ResolvedPropertySetter> SetProperties { get; set; }

        public IStyleApplicator Applicator => new StyleApplicator(this);

        public abstract Type ControlType { get; }

        public bool ExactTypeMatch { get; protected set; }

        public abstract bool Matches(StyleMatchContext matcher);

        protected virtual void ApplyStyle(ResolvedControl control)
        {
            if (SetHtmlAttributes != null)
            {
                foreach (var attr in SetHtmlAttributes)
                {
                    if (!control.HtmlAttributes.ContainsKey(attr.Key) || attr.Value.append)
                    {
                        ResolvedHtmlAttributeSetter setter = null;
                        if (attr.Value.value is ResolvedBinding)
                        {
                            setter = new ResolvedHtmlAttributeBinding(attr.Key, (ResolvedBinding)attr.Value.value);
                        }
                        else
                        {
                            setter = new ResolvedHtmlAttributeValue(attr.Key, (string)attr.Value.value);
                        }

                        control.SetHtmlAttribute(setter);
                    }
                }
            }
            if (SetProperties != null)
            {
                foreach (var prop in SetProperties)
                {
                    if (!control.Properties.ContainsKey(prop.Key))
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

        public struct HtmlAttributeInsertionInfo
        {
            public object value;
            public bool append;
        }
    }
}
