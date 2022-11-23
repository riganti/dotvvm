﻿using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

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
        var selectorDataSourceBinding = SelectorHelper.DiscoverSelectorDataSourceBinding(context, selectorConfiguration.SelectionType);

        return new Repeater()
            .SetCapability(props.Html)
            .SetProperty(c => c.WrapperTagName, "ul")
            .SetProperty(c => c.DataSource, selectorDataSourceBinding)
            .SetProperty(c => c.ItemTemplate, new CloneTemplate(
                new HtmlGenericControl("li")
                    .SetProperty(Internal.DataContextTypeProperty, context.CreateChildDataContextStack(context.DataContextStack, selectorConfiguration.SelectionType))
                    .AppendChildren(
                        new CheckBox()
                            .SetProperty(c => c.Text, context.CreateValueBinding("DisplayName", selectorConfiguration.SelectionType))
                            .SetProperty(c => c.CheckedValue, context.CreateValueBinding("Value", selectorConfiguration.SelectionType))
                            .SetProperty(c => c.CheckedItems, props.Property)
                            .SetProperty(c => c.Enabled, props.Enabled)
                            .SetProperty(c => c.Changed, props.Changed)
                    )
                )
            )
            .SetProperty(BootstrapForm.RequiresFormCheckCssClassProperty, true);
    }

}
