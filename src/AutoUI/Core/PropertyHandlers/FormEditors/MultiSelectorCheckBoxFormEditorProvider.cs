using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.AutoUI.PropertyHandlers.FormEditors;

public class MultiSelectorCheckBoxFormEditorProvider : FormEditorProviderBase
{
    public override bool CanHandleProperty(PropertyDisplayMetadata property, AutoUIContext context)
    {
        return property.SelectorConfiguration != null
               && ReflectionUtils.IsCollection(property.Type) && ReflectionUtils.IsPrimitiveType(ReflectionUtils.GetEnumerableType(property.Type)!);
    }

    public override DotvvmControl CreateControl(PropertyDisplayMetadata property, AutoEditor.Props props, AutoUIContext context)
    {
        var selectorConfiguration = property.SelectorConfiguration!;
        var selectorDataSourceBinding = SelectorHelper.DiscoverSelectorDataSourceBinding(context, selectorConfiguration.PropertyType);

        return new Repeater()
            .SetCapability(props.Html)
            .SetProperty(c => c.DataSource, selectorDataSourceBinding)
            .SetProperty(c => c.ItemTemplate, new CloneTemplate(
                new CheckBox()
                    .SetProperty(c => c.Text, context.CreateValueBinding("DisplayName", selectorConfiguration.PropertyType))
                    .SetProperty(c => c.CheckedValue, context.CreateValueBinding("Value", selectorConfiguration.PropertyType))
                    .SetProperty(c => c.CheckedItems, props.Property)
                    .SetProperty(c => c.Enabled, props.Enabled)
                    .SetProperty(c => c.Changed, props.Changed)
                    .SetProperty(Internal.DataContextTypeProperty, context.DataContextStack.CreateChildStack(selectorConfiguration.PropertyType))
                ));
    }

}
