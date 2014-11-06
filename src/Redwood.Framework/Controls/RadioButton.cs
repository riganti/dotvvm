using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Represents a radio button control.
    /// </summary>
    public class RadioButton : HtmlGenericControl
    {

        /// <summary>
        /// Gets or sets the label text that is rendered next to the control.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly RedwoodProperty TextProperty =
            RedwoodProperty.Register<string, RadioButton>(t => t.Text, "");


        /// <summary>
        /// Gets or sets whether the <see cref="RadioButton" /> is checked.
        /// </summary>
        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }
        public static readonly RedwoodProperty CheckedProperty =
            RedwoodProperty.Register<bool, RadioButton>(t => t.Checked, false);



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
        /// Gets or sets the value that will be stored in the <see cref="CheckedItem"/> property when this <see cref="RadioButton"/> is checked.
        /// </summary>
        public object CheckedValue
        {
            get { return GetValue(CheckedValueProperty); }
            set { SetValue(CheckedValueProperty, value); }
        }
        public static readonly RedwoodProperty CheckedValueProperty =
            RedwoodProperty.Register<object, RadioButton>(t => t.CheckedValue, null);


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


        /// <summary>
        /// Initializes a new instance of the <see cref="CheckBox"/> class.
        /// </summary>
        public RadioButton()
            : base("input")
        {
        }

        /// <summary>
        /// Renders the children.
        /// </summary>
        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            // label
            var textBinding = GetBinding(TextProperty);
            var labelRequired = textBinding != null || !string.IsNullOrEmpty(Text);
            if (labelRequired)
            {
                writer.RenderBeginTag("label");
            }

            // render the radio button
            var checkedItemBinding = GetBinding(CheckedItemProperty);
            if (checkedItemBinding != null)
            {
                // selected item mode
                writer.AddKnockoutDataBind("checked", checkedItemBinding as ValueBindingExpression);

                var checkedValueBinding = GetBinding(CheckedValueProperty);
                if (checkedValueBinding != null)
                {
                    writer.AddKnockoutDataBind("checkedValue", checkedValueBinding as ValueBindingExpression);
                }
                else
                {
                    var checkedValue = (CheckedValue ?? string.Empty).ToString();
                    if (!string.IsNullOrEmpty(checkedValue))
                    {
                        writer.AddKnockoutDataBind("checkedValue", KnockoutHelper.MakeStringLiteral(checkedValue.ToString()));
                    }
                }
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
                writer.AddKnockoutDataBind("attr", new[] { new KeyValuePair<string, ValueBindingExpression>("name", groupNameBinding as ValueBindingExpression) });
            }
            else
            {
                writer.AddAttribute("name", GroupName);
            }

            base.Render(writer, context);

            // render the label
            if (labelRequired)
            {
                if (textBinding != null)
                {
                    writer.AddKnockoutDataBind("text", textBinding as ValueBindingExpression);
                    writer.RenderBeginTag("span");
                    writer.RenderEndTag();
                }
                else if (!string.IsNullOrEmpty(Text))
                {
                    writer.WriteText(Text);
                }

                writer.RenderEndTag();
            }
        }
    }
}