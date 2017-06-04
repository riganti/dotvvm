using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Compilation.Styles
{
    public abstract class CompileTimeStyleBase : IStyle
    {
        public Dictionary<DotvvmProperty, PropertyInsertionInfo> SetProperties { get; } = new Dictionary<DotvvmProperty, PropertyInsertionInfo>();

        public IStyleApplicator Applicator => new StyleApplicator(this);

        public abstract Type ControlType { get; }

        public bool ExactTypeMatch { get; protected set; }

        public abstract bool Matches(StyleMatchContext matcher);

        protected virtual void ApplyStyle(ResolvedControl control, DotvvmConfiguration configuration)
        {
            if (SetProperties != null)
            {
                foreach (var prop in SetProperties)
                {
                    if (!control.Properties.ContainsKey(prop.Key) || prop.Value.Append)
                    {
                        control.SetProperty(prop.Value.Value, prop.Value.Append, out string error);
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

            public void ApplyStyle(ResolvedControl control, DotvvmConfiguration configuration)
            {
                @this.ApplyStyle(control, configuration);
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
