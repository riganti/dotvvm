using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation.Styles
{
    public abstract class CompileTimeStyleBase : IStyle
    {
        internal Dictionary<DotvvmProperty, PropertyInsertionInfo> SetProperties { get; } = new Dictionary<DotvvmProperty, PropertyInsertionInfo>();

        public IStyleApplicator Applicator => new StyleApplicator(this);

        public abstract Type ControlType { get; }

        public bool ExactTypeMatch { get; protected set; }

        public abstract bool Matches(StyleMatchContext matcher);

        protected virtual void ApplyStyle(ResolvedControl control)
        {
            //if (SetHtmlAttributes != null)
            //{
            //    foreach (var attr in SetHtmlAttributes)
            //    {
            //        if (!control.HtmlAttributes.ContainsKey(attr.Key) || attr.Value.append)
            //        {
            //            ResolvedHtmlAttributeSetter setter = null;
            //            if (attr.Value.value is ResolvedBinding)
            //            {
            //                setter = new ResolvedHtmlAttributeBinding(attr.Key, (ResolvedBinding)attr.Value.value);
            //            }
            //            else
            //            {
            //                setter = new ResolvedHtmlAttributeValue(attr.Key, (string)attr.Value.value);
            //            }

            //            control.SetHtmlAttribute(setter);
            //        }
            //    }
            //}
            if (SetProperties != null)
            {
                foreach (var prop in SetProperties)
                {
                    if (!control.Properties.ContainsKey(prop.Key))
                    {
                        string error;
                        control.SetProperty(prop.Value.Value, prop.Value.Append, out error);
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

        public struct PropertyInsertionInfo
        {
            public readonly ResolvedPropertySetter Value;
            public readonly bool Append;

            public PropertyInsertionInfo(ResolvedPropertySetter value, bool append)
            {
                this.Value = value;
                this.Append = append;
            }
        }
    }
}
