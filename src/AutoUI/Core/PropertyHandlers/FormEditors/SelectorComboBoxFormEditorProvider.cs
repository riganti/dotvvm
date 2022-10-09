using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.AutoUI.PropertyHandlers.FormEditors
{
    public class SelectorComboBoxFormEditorProvider : FormEditorProviderBase
    {
        public override bool CanHandleProperty(PropertyDisplayMetadata property, AutoUIContext context)
        {
            return property.SelectionConfiguration != null
                && ReflectionUtils.IsPrimitiveType(property.Type);
        }

        public override DotvvmControl CreateControl(PropertyDisplayMetadata property, AutoEditor.Props props, AutoUIContext context)
        {
            var selectorConfiguration = property.SelectionConfiguration!;
            var selectorDataSourceBinding = SelectorHelper.DiscoverSelectorDataSourceBinding(context, selectorConfiguration.PropertyType);

            return new ComboBox()
                .SetCapability(props.Html)
                .SetProperty(c => c.DataSource, selectorDataSourceBinding)
                .SetProperty(c => c.ItemTextBinding, context.CreateValueBinding("DisplayName", selectorConfiguration.PropertyType))
                .SetProperty(c => c.ItemValueBinding, context.CreateValueBinding("Value", selectorConfiguration.PropertyType))
                .SetProperty(c => c.SelectedValue, props.Property)
                .SetProperty(c => c.Enabled, props.Enabled)
                .SetProperty(c => c.SelectionChanged, props.Changed);
        }

    }
}
