using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
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
        public static readonly RedwoodProperty TextProperty =
            RedwoodProperty.Register<string, CheckableControlBase>(t => t.Text, "");

        /// <summary>
        /// Gets or sets whether the <see cref="RadioButton" /> is checked.
        /// </summary>
        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }
        public static readonly RedwoodProperty CheckedProperty =
            RedwoodProperty.Register<bool, CheckableControlBase>(t => t.Checked, false);

        /// <summary>
        /// Gets or sets the value that will be used as a result when the control is checked.
        /// </summary>
        public object CheckedValue
        {
            get { return GetValue(CheckedValueProperty); }
            set { SetValue(CheckedValueProperty, value); }
        }
        public static readonly RedwoodProperty CheckedValueProperty =
            RedwoodProperty.Register<object, CheckableControlBase>(t => t.CheckedValue, null);


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
            var textBinding = GetBinding(TextProperty);
            var labelRequired = textBinding != null || !string.IsNullOrEmpty(Text) || !HasOnlyWhiteSpaceContent();
            if (labelRequired)
            {
                writer.RenderBeginTag("label");
            }

            // render the radio button
            RenderInputTag(writer);

            // render the label
            if (labelRequired)
            {
                if (textBinding != null)
                {
                    writer.AddKnockoutDataBind("text", this, TextProperty, () => { });
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