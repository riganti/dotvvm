using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

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
            var selectorDiscoveryService = context.Services.GetRequiredService<ISelectorDiscoveryService>();
            var selectorDataSourceBinding = selectorDiscoveryService.DiscoverSelectorDataSourceBinding(context, selectorConfiguration.SelectionType);
            var nestedDataContext = context.CreateChildDataContextStack(selectorConfiguration.SelectionType);

            return new ComboBox()
                .SetCapability(props.Html)
                .SetProperty(c => c.DataSource, selectorDataSourceBinding)
                .SetProperty(c => c.ItemTextBinding, context.BindingService.Cache.CreateValueBinding<string>("DisplayName", nestedDataContext))
                .SetProperty(c => c.ItemValueBinding, context.BindingService.Cache.CreateValueBinding("Value", nestedDataContext))
                .SetProperty(c => c.SelectedValue, props.Property)
                .SetProperty(c => c.Enabled, props.Enabled)
                .SetProperty(c => c.SelectionChanged, props.Changed);
        }

    }
}
