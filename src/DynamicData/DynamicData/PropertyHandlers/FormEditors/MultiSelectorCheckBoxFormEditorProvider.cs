using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors;

public class MultiSelectorCheckBoxFormEditorProvider : FormEditorProviderBase
{
    public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
    {
        return context.PropertyDisplayMetadataProvider.GetPropertyMetadata(propertyInfo).SelectorConfiguration != null
               && ReflectionUtils.IsCollection(propertyInfo.PropertyType) && ReflectionUtils.IsPrimitiveType(ReflectionUtils.GetEnumerableType(propertyInfo.PropertyType)!);
    }

    public override DotvvmControl CreateControl(PropertyDisplayMetadata property, DynamicEditor.Props props, DynamicDataContext context)
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
