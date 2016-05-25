using System;

namespace DotVVM.Framework.Controls
{
    static internal class SelectHtmlControlHelpers
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
            writer.AddKnockoutDataBind("options", selector, ItemsControl.DataSourceProperty, renderEvenInServerRenderingMode: true);
            if (!String.IsNullOrEmpty(selector.DisplayMember))
            {
                writer.AddKnockoutDataBind("optionsText", "function (i) { return ko.unwrap(i)[" + KnockoutHelper.MakeStringLiteral(selector.DisplayMember) + "]; }");
            }
            if (!String.IsNullOrEmpty(selector.ValueMember))
            {
                writer.AddKnockoutDataBind("optionsValue", "function (i) { return ko.unwrap(i)[" + KnockoutHelper.MakeStringLiteral(selector.ValueMember) + "]; }");
            }
        }

        public static void RenderChangedEvent(IHtmlWriter writer, SelectorBase selector)
        {
            var selectionChangedBinding = selector.GetCommandBinding(SelectorBase.SelectionChangedProperty);
            if (selectionChangedBinding != null)
            {
                writer.AddAttribute("onchange", KnockoutHelper.GenerateClientPostBackScript(nameof(SelectorBase.SelectionChanged), selectionChangedBinding, selector, isOnChange: true, useWindowSetTimeout: true));
            }
        }
    }
}