using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the HTML checkbox control.
    /// </summary>
    public class CheckBox : CheckableControlBase
    {

        /// <summary>
        /// Gets or sets a collection of values of all checked checkboxes. Use this property in combination with the CheckedValue property.
        /// </summary>
        public IEnumerable CheckedItems
        {
            get { return (IEnumerable)GetValue(CheckedItemsProperty); }
            set { SetValue(CheckedItemsProperty, value); }
        }
        public static readonly DotvvmProperty CheckedItemsProperty =
            DotvvmProperty.Register<IEnumerable, CheckBox>(t => t.CheckedItems, null);
        

        /// <summary>
        /// Renders the input tag.
        /// </summary>
        protected override void RenderInputTag(IHtmlWriter writer)
        {
            if (HasValueBinding(CheckedProperty) && !HasValueBinding(CheckedItemsProperty))
            {
                // boolean mode
                RenderCheckedProperty(writer);
            }
            else if (!HasValueBinding(CheckedProperty) && HasValueBinding(CheckedItemsProperty))
            {
                // collection mode
                RenderCheckedItemsProperty(writer);
            }
            else
            {
                throw new DotvvmControlException(this, "Either the Checked or the CheckedItems binding of a CheckBox must be set.");
            }

            RenderTypeAttribute(writer);
            writer.RenderSelfClosingTag("input");
        }

        protected virtual void RenderTypeAttribute(IHtmlWriter writer)
        {
            // render the input tag
            writer.AddAttribute("type", "checkbox");
        }

        protected virtual void RenderCheckedItemsProperty(IHtmlWriter writer)
        {
            RenderCheckedItemsBinding(writer);
            writer.AddKnockoutDataBind("checkedArrayContainsObservables", "true");
            writer.AddKnockoutDataBind("dotvvm-checkbox-updateAfterPostback", "true");
            RenderDotvvmCheckedPointerBinding(writer);
            writer.AddKnockoutDataBind("checkedValue", this, CheckedValueProperty, () =>
            {
                var checkedValue = (CheckedValue ?? string.Empty).ToString();
                if (!string.IsNullOrEmpty(checkedValue))
                {
                    writer.AddKnockoutDataBind("checkedValue", KnockoutHelper.MakeStringLiteral(checkedValue));
                }
            });
        }

        protected virtual void RenderDotvvmCheckedPointerBinding(IHtmlWriter writer)
        {
            writer.AddKnockoutDataBind("dotvvm-checked-pointer", GetDotvvmCheckedPointerBindingValue());
        }

        protected virtual string GetDotvvmCheckedPointerBindingValue()
        {
            return "'checked'";
        }

        protected virtual void RenderCheckedItemsBinding(IHtmlWriter writer)
        {
            var checkedItemsBinding = GetValueBinding(CheckedItemsProperty);
            writer.AddKnockoutDataBind("checked", checkedItemsBinding);
        }

        protected virtual void RenderCheckedProperty(IHtmlWriter writer)
        {
            var checkedBinding = GetValueBinding(CheckedProperty);
            writer.AddKnockoutDataBind("checked", checkedBinding);
            writer.AddKnockoutDataBind("checkedValue", "true");

            // Boolean mode can have prerendered `checked` attribute
            if (RenderOnServer && true.Equals(GetValue(CheckedProperty)))
                writer.AddAttribute("checked", null);
        }
    }
}
