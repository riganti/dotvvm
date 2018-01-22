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
                    if (!control.Properties.ContainsKey(prop.Key)
                        || prop.Value.Type == StyleOverrideOptions.Append
                        || prop.Value.Type == StyleOverrideOptions.Overwrite)
                    {
                        control.SetProperty(prop.Value.Value, prop.Value.Type == StyleOverrideOptions.Overwrite, out string error);
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
            public readonly StyleOverrideOptions Type;

            public PropertyInsertionInfo(ResolvedPropertySetter value, StyleOverrideOptions type)
            {
                this.Value = value;
                this.Type = type;
            }
        }
    }
}
