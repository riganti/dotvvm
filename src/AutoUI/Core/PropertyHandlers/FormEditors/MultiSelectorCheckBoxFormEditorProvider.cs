using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.AutoUI.PropertyHandlers.FormEditors;

public class MultiSelectorCheckBoxFormEditorProvider : FormEditorProviderBase
{
    public override bool CanHandleProperty(PropertyDisplayMetadata property, AutoUIContext context)
    {
        return property.SelectionConfiguration != null
               && ReflectionUtils.IsCollection(property.Type) && ReflectionUtils.IsPrimitiveType(ReflectionUtils.GetEnumerableType(property.Type)!);
    }

    public override DotvvmControl CreateControl(PropertyDisplayMetadata property, AutoEditor.Props props, AutoUIContext context)
    {
        var selectorConfiguration = property.SelectionConfiguration!;
        var selectorDiscoveryService = context.Services.GetRequiredService<ISelectorDiscoveryService>();
        var selectorDataSourceBinding = selectorDiscoveryService.DiscoverSelectorDataSourceBinding(context, selectorConfiguration.SelectionType);
        var repeater = new Repeater()
            .SetCapability(props.Html)
            .SetProperty(c => c.WrapperTagName, "ul")
            .SetProperty(c => c.DataSource, selectorDataSourceBinding);

        var nestedDataContext = BindingHelper.GetDataContextType(Repeater.ItemTemplateProperty, repeater, context.DataContextStack).NotNull();

        return repeater
            .SetProperty(c => c.ItemTemplate, new CloneTemplate(
                new HtmlGenericControl("li")
                    .AppendChildren(
                        new CheckBox()
                            .SetProperty(c => c.Text, context.BindingService.Cache.CreateValueBinding<string>("DisplayName", nestedDataContext))
                            .SetProperty(c => c.CheckedValue, context.BindingService.Cache.CreateValueBinding("Value", nestedDataContext))
                            .SetProperty(c => c.CheckedItems, props.Property)
                            .SetProperty(c => c.Enabled, props.Enabled)
                            .SetProperty(c => c.Changed, props.Changed)
                    )
                )
            )
            .SetProperty(BootstrapForm.RequiresFormCheckCssClassProperty, true);
    }

}
