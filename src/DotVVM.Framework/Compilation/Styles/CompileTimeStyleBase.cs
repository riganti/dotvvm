using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using System.Linq;

namespace DotVVM.Framework.Compilation.Styles
{
    public abstract class CompileTimeStyleBase : IStyle
    {
        public Dictionary<DotvvmProperty, IPropertyInsertionInfo> SetProperties { get; } = new Dictionary<DotvvmProperty, IPropertyInsertionInfo>();

        public IStyleApplicator Applicator => new StyleApplicator(this);

        public abstract Type ControlType { get; }

        public bool ExactTypeMatch { get; protected set; }

        public abstract bool Matches(StyleMatchContext context);

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
                        control.SetProperty(prop.Value.GetPropertySetter(control, configuration),
                            prop.Value.Type == StyleOverrideOptions.Overwrite, out string error);
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

        public class PropertyInsertionInfo : IPropertyInsertionInfo
        {
            private readonly ResolvedPropertySetter value;
            public StyleOverrideOptions Type { get; }

            public PropertyInsertionInfo(ResolvedPropertySetter value, StyleOverrideOptions type)
            {
                this.value = value;
                this.Type = type;
            }

            public ResolvedPropertySetter GetPropertySetter(ResolvedControl resolvedControl, DotvvmConfiguration configuration)
            {
                return value;
            }
        }

        public class PropertyControlCollectionInsertionInfo : IPropertyInsertionInfo
        {
            private readonly DotvvmProperty dotvvmProperty;
            private readonly ControlResolverMetadata metadata;
            private readonly IStyle innerControlStyle;

            public StyleOverrideOptions Type { get; }

            public PropertyControlCollectionInsertionInfo(DotvvmProperty dotvvmProperty, StyleOverrideOptions type,
                ControlResolverMetadata metadata, IStyle innerControlStyle)
            {
                this.dotvvmProperty = dotvvmProperty;
                this.Type = type;
                this.metadata = metadata;
                this.innerControlStyle = innerControlStyle;
            }

            public ResolvedPropertySetter GetPropertySetter(ResolvedControl resolvedControl, DotvvmConfiguration configuration)
            {
                var resolvedInnerControl = new ResolvedControl(metadata, null, resolvedControl.DataContextTypeStack);
                innerControlStyle.Applicator.ApplyStyle(resolvedInnerControl, configuration);

                return new ResolvedPropertyControlCollection(dotvvmProperty, new List<ResolvedControl> { resolvedInnerControl });
            }
        }
    }
}
