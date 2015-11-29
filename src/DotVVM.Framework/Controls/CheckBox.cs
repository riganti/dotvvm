using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Exceptions;

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
            var checkedBinding = GetValueBinding(CheckedProperty);
            var checkedItemsBinding = GetValueBinding(CheckedItemsProperty);

            if (checkedBinding != null && checkedItemsBinding == null)
            {
                // boolean mode
                writer.AddKnockoutDataBind("checked", checkedBinding);
                writer.AddKnockoutDataBind("checkedValue", "true");
            }
            else if (checkedBinding == null && checkedItemsBinding != null)
            {
                // collection mode
                writer.AddKnockoutDataBind("checked", checkedItemsBinding);
                writer.AddKnockoutDataBind("checkedArrayContainsObservables", "true");
                writer.AddKnockoutDataBind("checkedValue", this, CheckedValueProperty, () =>
                {
                    var checkedValue = (CheckedValue ?? string.Empty).ToString();
                    if (!string.IsNullOrEmpty(checkedValue))
                    {
                        writer.AddKnockoutDataBind("checkedValue", KnockoutHelper.MakeStringLiteral(checkedValue));
                    }
                });
            }
            else
            {
                throw new DotvvmControlException(this, "Either the Checked or the CheckedItems binding of a CheckBox must be set.");
            }

            // render the input tag
            writer.AddAttribute("type", "checkbox");
            writer.RenderSelfClosingTag("input");
        }
    }
}
