using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Represents a checkbox control.
    /// </summary>
    public class CheckBox : HtmlGenericControl
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
            RedwoodProperty.Register<string, CheckBox>(t => t.Text, "");


        /// <summary>
        /// Gets or sets whether the <see cref="CheckBox" /> is checked.
        /// </summary>
        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }
        public static readonly RedwoodProperty CheckedProperty =
            RedwoodProperty.Register<bool, CheckBox>(t => t.Checked, false);



        /// <summary>
        /// Gets or sets the <see cref="CheckedValue"/>s of all <see cref="CheckBox">CheckBoxes</see> that are checked and bound to this collection.
        /// </summary>
        public IEnumerable CheckedItems
        {
            get { return (IEnumerable)GetValue(CheckedItemsProperty); }
            set { SetValue(CheckedItemsProperty, value); }
        }
        public static readonly RedwoodProperty CheckedItemsProperty =
            RedwoodProperty.Register<IEnumerable, CheckBox>(t => t.CheckedItems, null);


        /// <summary>
        /// Gets or sets the value that will be stored in the <see cref="CheckedItems"/> collection when this <see cref="CheckBox"/> is checked.
        /// </summary>
        public object CheckedValue
        {
            get { return GetValue(CheckedValueProperty); }
            set { SetValue(CheckedValueProperty, value); }
        }
        public static readonly RedwoodProperty CheckedValueProperty =
            RedwoodProperty.Register<object, CheckBox>(t => t.CheckedValue, null);


        /// <summary>
        /// Initializes a new instance of the <see cref="CheckBox"/> class.
        /// </summary>
        public CheckBox() : base("input")
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

            // render the checkbox
            var checkedBinding = GetBinding(CheckedProperty);
            var checkedItemsBinding = GetBinding(CheckedItemsProperty);

            if (checkedBinding != null && checkedItemsBinding == null)
            {
                // boolean mode
                writer.AddKnockoutDataBind("checked", checkedBinding as ValueBindingExpression);
                writer.AddKnockoutDataBind("checkedValue", "true");
            }
            else if (checkedBinding == null && checkedItemsBinding != null)
            {
                // collection mode
                writer.AddKnockoutDataBind("checked", checkedItemsBinding as ValueBindingExpression);

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
                throw new Exception("Either the Checked or the CheckedItems property of a CheckBox must be set.");                
            }

            // render the input tag
            writer.AddAttribute("type", "checkbox");
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
