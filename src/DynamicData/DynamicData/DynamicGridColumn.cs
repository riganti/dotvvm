using System;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Binding.Properties;

namespace DotVVM.Framework.Controls.DynamicData
{
    public class DynamicGridColumn: GridViewColumn
    {
        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public IValueBinding? Property
        {
            get { return (IValueBinding?)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }
        public static readonly DotvvmProperty PropertyProperty =
            DotvvmProperty.Register<IValueBinding, DynamicGridColumn>(nameof(Property));


        public static DotvvmCapabilityProperty PropsProperty =
            DotvvmCapabilityProperty.RegisterCapability<Props, DynamicGridColumn>();

        public static GridViewColumn Replace(IStyleMatchContext<DynamicGridColumn> col)
        {
            var context = new DynamicDataContext(col.Control.DataContextTypeStack, col.Configuration.ServiceProvider);

            var props = col.PropertyValue<Props>(DynamicGridColumn.PropsProperty).NotNull();
            if (props.Property is null)
                throw new DotvvmControlException($"DynamicGridColumn.Property is not set.");


            var prop = props.Property.GetProperty<ReferencedViewModelPropertiesBindingProperty>();

            if (prop.MainProperty is null)
                throw new NotSupportedException($"The binding {props.Property} must be bound to a single property. Alternatively, you can write a custom server-side style rule for your expression.");

            var propertyMetadata = context.PropertyDisplayMetadataProvider.GetPropertyMetadata(prop.MainProperty);

            var provider =
                context.DynamicDataConfiguration.GridColumnProviders
                    .FirstOrDefault(e => e.CanHandleProperty(prop.MainProperty, context));

            if (provider is null)
                throw new DotvvmControlException($"GridViewColumn provider for property {prop.MainProperty} could not be found.");

            if (props.HeaderTemplate is null && props.HeaderText is null)
            {
                props = props with { HeaderText = propertyMetadata.DisplayName?.ToBinding(context) ?? new(propertyMetadata.PropertyInfo.Name) };
            }

            var control = provider.CreateColumn(propertyMetadata, props, context);
            if (!control.IsPropertySet(HeaderTextProperty) && !control.IsPropertySet(HeaderTemplateProperty))
            {
                control.SetValue(HeaderTextProperty, props.HeaderText);
            }

            // editor

            if (props.EditTemplate is null)
            {
                control.EditTemplate = new CloneTemplate(
                    new DynamicEditor(context.Services)
                        .SetProperty(p => p.Property, props.Property)
                        .SetProperty("Enabled", props.IsEditable)
                );
            }
            else
                control.EditTemplate = props.EditTemplate;

            return control;

        }

        public override void CreateControls(IDotvvmRequestContext context, DotvvmControl container) => throw new NotImplementedException("DynamicGridColumn must be replaced using server-side styles. It cannot be used at runtime");
        public override void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container) => throw new NotImplementedException("DynamicGridColumn must be replaced using server-side styles. It cannot be used at runtime");

        [DotvvmControlCapability]
        public sealed record Props
        {
            public IValueBinding? Property { get; init; }
            public ValueOrBinding<bool> IsEditable { get; init; } = new(true);
            public ValueOrBinding<string>? HeaderText { get; init; }
            public ITemplate? HeaderTemplate { get; init; }
            public ITemplate? EditTemplate { get; init; }
        }
    }
}
