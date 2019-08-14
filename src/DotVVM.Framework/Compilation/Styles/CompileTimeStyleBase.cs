using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

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
            enum PropertyKind
            {
                Template,
                Collection,
                SingleControl
            }

            private readonly DotvvmProperty dotvvmProperty;
            private readonly PropertyKind propertyKind;
            private readonly ControlResolverMetadata metadata;
            private readonly IStyle innerControlStyle;
            private readonly object[] ctorParameters;

            public StyleOverrideOptions Type { get; }

            public PropertyControlCollectionInsertionInfo(DotvvmProperty dotvvmProperty, StyleOverrideOptions type,
                ControlResolverMetadata metadata, IStyle innerControlStyle, object[] ctorParameters)
            {
                this.dotvvmProperty = dotvvmProperty;
                this.Type = type;
                this.metadata = metadata;
                this.innerControlStyle = innerControlStyle;
                this.ctorParameters = ctorParameters;
                this.propertyKind = DeterminePropertyKind(dotvvmProperty, metadata);
            }

            static PropertyKind DeterminePropertyKind(DotvvmProperty property, ControlResolverMetadata controlMetadata)
            {
                var propType = property.PropertyType;
                if (typeof(ITemplate).IsAssignableFrom(propType))
                    return PropertyKind.Template;
                else if (typeof(System.Collections.ICollection).IsAssignableFrom(propType) &&
                         ReflectionUtils.GetEnumerableType(propType).IsAssignableFrom(controlMetadata.Type))
                    return PropertyKind.Collection;
                else if (typeof(DotvvmBindableObject).IsAssignableFrom(propType) &&
                         propType.IsAssignableFrom(controlMetadata.Type))
                    return PropertyKind.SingleControl;
                else
                    throw new Exception($"Can not set a control of type {controlMetadata.Type} to a property of type {propType}.");
            }

            public ResolvedPropertySetter GetPropertySetter(ResolvedControl resolvedControl, DotvvmConfiguration configuration)
            {
                var resolvedInnerControl = new ResolvedControl(metadata, null, resolvedControl.DataContextTypeStack);
                resolvedInnerControl.ConstructorParameters = this.ctorParameters;
                innerControlStyle.Applicator.ApplyStyle(resolvedInnerControl, configuration);

                if (this.propertyKind == PropertyKind.Template)
                    return new ResolvedPropertyTemplate(dotvvmProperty, new List<ResolvedControl> { resolvedInnerControl });
                else if (this.propertyKind == PropertyKind.Collection)
                    return new ResolvedPropertyControlCollection(dotvvmProperty, new List<ResolvedControl> { resolvedInnerControl });
                else if (this.propertyKind == PropertyKind.SingleControl)
                    return new ResolvedPropertyControl(dotvvmProperty, resolvedInnerControl);
                else
                    throw new Exception();
            }
        }
    }
}
