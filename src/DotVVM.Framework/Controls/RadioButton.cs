using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a HTML radio button.
    /// </summary>
    public class RadioButton : CheckableControlBase
    {
        /// <summary>
        /// Gets or sets the <see cref="CheckableControlBase.CheckedValue"/> of the first <see cref="RadioButton" /> that is checked and bound to this collection.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public object CheckedItem
        {
            get { return GetValue(CheckedItemProperty); }
            set { SetValue(CheckedItemProperty, value); }
        }
        public static readonly DotvvmProperty CheckedItemProperty =
            DotvvmProperty.Register<object, RadioButton>(t => t.CheckedItem, null);

        /// <summary>
        /// Gets or sets an unique name of the radio button group.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }
        public static readonly DotvvmProperty GroupNameProperty =
            DotvvmProperty.Register<string, RadioButton>(t => t.GroupName, "");



        protected override void RenderInputTag(IHtmlWriter writer)
        {
            var checkedItemBinding = GetValueBinding(CheckedItemProperty);
            if (checkedItemBinding != null)
            {
                // selected item mode
                writer.AddKnockoutDataBind("checked", checkedItemBinding);
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
                writer.AddKnockoutDataBind("checked", this, CheckedProperty, () => { });
            }

            // render the input tag
            writer.AddAttribute("type", "radio");

            var groupNameBinding = GetValueBinding(GroupNameProperty);
            if (groupNameBinding != null)
            {
                // TODO: do not overwrite existing attribute bindings
                writer.AddKnockoutDataBind("attr", new[] { new KeyValuePair<string, IValueBinding>("name", groupNameBinding) }, this, GroupNameProperty);
            }
            else
            {
                writer.AddAttribute("name", GroupName);
            }

            writer.RenderSelfClosingTag("input");
        }
    }
}