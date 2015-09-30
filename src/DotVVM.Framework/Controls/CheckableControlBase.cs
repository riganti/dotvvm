using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A base control for checkbox and radiobutton controls.
    /// </summary>
    public abstract class CheckableControlBase : HtmlGenericControl
    {
        
        /// <summary>
        /// Gets or sets the label text that is rendered next to the control.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<string, CheckableControlBase>(t => t.Text, "");

        /// <summary>
        /// Gets or sets whether the control is checked.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }
        public static readonly DotvvmProperty CheckedProperty =
            DotvvmProperty.Register<bool, CheckableControlBase>(t => t.Checked, false);

        /// <summary>
        /// Gets or sets the value that will be used as a result when the control is checked.
        /// Use this property in combination with the CheckedValue or CheckedValues property.
        /// </summary>
        public object CheckedValue
        {
            get { return GetValue(CheckedValueProperty); }
            set { SetValue(CheckedValueProperty, value); }
        }
        public static readonly DotvvmProperty CheckedValueProperty =
            DotvvmProperty.Register<object, CheckableControlBase>(t => t.CheckedValue, null);



        /// <summary>
        /// Gets or sets the command that will be triggered when the control check state is changed.
        /// </summary>
        public Action Changed
        {
            get { return (Action)GetValue(ChangedProperty); }
            set { SetValue(ChangedProperty, value); }
        }
        public static readonly DotvvmProperty ChangedProperty =
            DotvvmProperty.Register<Action, CheckableControlBase>(t => t.Changed, null);



        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled and can be clicked on.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty =
            DotvvmProperty.Register<bool, CheckableControlBase>(t => t.Enabled, true);




        /// <summary>
        /// Initializes a new instance of the <see cref="CheckableControlBase"/> class.
        /// </summary>
        public CheckableControlBase() : base("span")
        {
            
        }

        /// <summary>
        /// Renders the children.
        /// </summary>
        protected override void RenderControl(IHtmlWriter writer, RenderContext context)
        {
            // label
            var textBinding = GetValueBinding(TextProperty);
            var labelRequired = textBinding != null || !string.IsNullOrEmpty(Text) || !HasOnlyWhiteSpaceContent();
            if (labelRequired)
            {
                writer.RenderBeginTag("label");
            }

            // prepare changed event attribute
            var changedBinding = GetCommandBinding(ChangedProperty);
            if (changedBinding != null)
            {
                writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript(changedBinding, context, this, true, true, isOnChange: true));
            }

            // handle enabled attribute
            writer.AddKnockoutDataBind("enable", this, EnabledProperty, () =>
            {
                if (!Enabled)
                {
                    writer.AddAttribute("disabled", "disabled");
                }
            });

            // add ID
            AddAttributesToRender(writer, context);

            // render the radio button
            RenderInputTag(writer);

            // render the label
            if (labelRequired)
            {
                if (textBinding != null)
                {
                    writer.AddKnockoutDataBind("text", textBinding);
                    writer.RenderBeginTag("span");
                    writer.RenderEndTag();
                }
                else if (!string.IsNullOrEmpty(Text))
                {
                    writer.WriteText(Text);
                }
                else if (!HasOnlyWhiteSpaceContent())
                {
                    RenderChildren(writer, context);
                }

                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Renders the input tag.
        /// </summary>
        protected abstract void RenderInputTag(IHtmlWriter writer);
    }
}