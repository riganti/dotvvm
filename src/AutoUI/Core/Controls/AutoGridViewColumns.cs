using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.AutoUI.Controls
{
    [ControlMarkupOptions(PrimaryName = "GridViewColumns")]
    public class AutoGridViewColumns : GridViewColumn
    {
        public static DotvvmCapabilityProperty PropsProperty =
            DotvvmCapabilityProperty.RegisterCapability<Props, AutoGridViewColumns>();

        public static GridViewColumn[] Replace(IStyleMatchContext<AutoGridViewColumns> col)
        {
            if (col.HasProperty(c => c.EditTemplate))
                throw new NotSupportedException("EditTemplate is not supported in DynamicGridColumnGroup.");

            var props = col.PropertyValue<Props>(PropsProperty).NotNull();

            var context = new AutoUIContext(col.Control.DataContextTypeStack, col.Configuration.ServiceProvider) {
                ViewName = props.FieldSelector.ViewName,
                GroupName = props.FieldSelector.GroupName
            };

            var properties = AutoFormBase.GetPropertiesToDisplay(context, props.FieldSelector);

            var columns = properties.Select(p => CreateColumn(p, context, props)).ToArray();
            return columns;
        }

        protected static AutoGridViewColumn CreateColumn(PropertyDisplayMetadata property, AutoUIContext context, Props props)
        {
            var name = property.Name;
            return
                new AutoGridViewColumn()
                    .SetProperty(p => p.HeaderText, props.Header.GetValueOrDefault(name)!)
                    .SetProperty(p => p.Property, context.CreateValueBinding(property))
                    .SetProperty("EditTemplate", props.EditorTemplate.GetValueOrDefault(name))
                    .SetProperty("ContentTemplate", props.ContentTemplate.GetValueOrDefault(name))
                    .SetProperty("Changed", props.Changed.GetValueOrDefault(name))
                    .SetProperty("IsEditable", props.IsEditable.GetValueOrDefault(name, property.IsEnabledBinding(context)));
        }

        public override void CreateControls(IDotvvmRequestContext context, DotvvmControl container) => throw new NotImplementedException("AutoGridViewColumn must be replaced using server-side styles. It cannot be used at runtime");
        public override void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container) => throw new NotImplementedException("AutoGridViewColumn must be replaced using server-side styles. It cannot be used at runtime");

        [DotvvmControlCapability]
        public sealed record Props
        {
            /// <summary> Calls the command when the user makes changes to the specified field. For example `Changed-CountryId="{staticCommand: _root.States.Items = statesDataProvider.GetSelectorItems(_root.Address).Result}"` will reload the list of states whenever CountryId is changed. </summary>
            [PropertyGroup("Changed-")]
            public IReadOnlyDictionary<string, ICommandBinding> Changed { get; init; } = new Dictionary<string, ICommandBinding>();

            /// <summary> Controls if the specified property is editable. </summary>
            [PropertyGroup("IsEditable-")]
            public IReadOnlyDictionary<string, ValueOrBinding<bool>> IsEditable { get; init; } = new Dictionary<string, ValueOrBinding<bool>>();

            /// <summary> Overrides which text is used as the column title. </summary>
            [PropertyGroup("Header-")]
            public IReadOnlyDictionary<string, ValueOrBinding<string>> Header { get; init; } = new Dictionary<string, ValueOrBinding<string>>();

            /// <summary> Overrides how the field is displayed. </summary>
            [PropertyGroup("ContentTemplate-")]
            [MarkupOptions(MappingMode = MappingMode.InnerElement)]
            public IReadOnlyDictionary<string, ITemplate> ContentTemplate { get; init; } = new Dictionary<string, ITemplate>();


            /// <summary> Overrides which component is used as an editor. </summary>
            [PropertyGroup("EditorTemplate-")]
            [MarkupOptions(MappingMode = MappingMode.InnerElement)]
            public IReadOnlyDictionary<string, ITemplate> EditorTemplate { get; init; } = new Dictionary<string, ITemplate>();

            public AutoFormBase.FieldSelectorProps FieldSelector { get; init; } = new();
        }
    }
}
