using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Renders a HTML radio button.
    /// </summary>
    public class RadioButton : CheckableControlBase
    {
        /// <summary>
        /// Gets or sets the <see cref="CheckedValue"/> of the first <see cref="RadioButton" /> that is checked and bound to this collection.
        /// </summary>
        public object CheckedItem
        {
            get { return GetValue(CheckedItemProperty); }
            set { SetValue(CheckedItemProperty, value); }
        }
        public static readonly RedwoodProperty CheckedItemProperty =
            RedwoodProperty.Register<IEnumerable, RadioButton>(t => t.CheckedItem, null);
         
        /// <summary>
        /// Gets or sets an unique name of the radio button group.
        /// </summary>
        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }
        public static readonly RedwoodProperty GroupNameProperty =
            RedwoodProperty.Register<string, RadioButton>(t => t.GroupName, "");

         

        protected override void RenderInputTag(IHtmlWriter writer)
        {
            var checkedItemBinding = GetBinding(CheckedItemProperty);
            if (checkedItemBinding != null)
            {
                // selected item mode
                writer.AddKnockoutDataBind("checked", this, CheckedItemProperty, () => { });
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
                throw new Exception("The CheckedItem property of a RadioButton must be set.");
            }

            // render the input tag
            writer.AddAttribute("type", "radio");

            var groupNameBinding = GetBinding(GroupNameProperty);
            if (groupNameBinding != null)
            {
                writer.AddKnockoutDataBind("attr", new[] { new KeyValuePair<string, ValueBindingExpression>("name", groupNameBinding as ValueBindingExpression) }, this, GroupNameProperty);
            }
            else
            {
                writer.AddAttribute("name", GroupName);
            }

            writer.RenderSelfClosingTag("input");
        }
    }
}