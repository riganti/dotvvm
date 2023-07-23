using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.Controls
{
    public static class SelectHtmlControlHelpers
    {
        public static void RenderEnabledProperty(IHtmlWriter writer, SelectorBase selector)
        {
            writer.AddKnockoutDataBind("enable", selector, SelectorBase.EnabledProperty, () =>
            {
                if (!selector.Enabled)
                {
                    writer.AddAttribute("disabled", "disabled");
                }
            });
        }

        public static void RenderOptionsProperties(IHtmlWriter writer, SelectorBase selector)
        {
            var optionsBinding = (IValueBinding?)selector.GetValueBinding(ItemsControl.DataSourceProperty)?.GetProperty<DataSourceAccessBinding>().Binding;
            if (optionsBinding is {})
            {
                writer.AddKnockoutDataBind("options", selector, optionsBinding);
            }
            if (selector.ItemTextBinding != null)
            {
                writer.AddKnockoutDataBind("optionsText", selector.ItemTextBinding.GetProperty<SelectorItemBindingProperty>().Expression, selector);
            }

            if (selector.ItemValueBinding != null)
            {
                writer.AddKnockoutDataBind("optionsValue", selector.ItemValueBinding.GetProperty<SelectorItemBindingProperty>().Expression, selector);
            }

            if (selector.ItemTitleBinding != null)
            {
                writer.AddKnockoutDataBind("optionsTitle", selector.ItemTitleBinding.GetProperty<SelectorItemBindingProperty>().Expression, selector);
            }
        }

        public static void RenderChangedEvent(IHtmlWriter writer, SelectorBase selector)
        {
            var selectionChangedBinding = selector.GetCommandBinding(SelectorBase.SelectionChangedProperty);
            if (selectionChangedBinding != null)
            {
                writer.AddAttribute("onchange", KnockoutHelper.GenerateClientPostBackScript(nameof(SelectorBase.SelectionChanged), selectionChangedBinding, selector, isOnChange: true, useWindowSetTimeout: true), true, ";");
            }
        }
    }
}
