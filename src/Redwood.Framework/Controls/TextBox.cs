using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Renders a HTML text input control.
    /// </summary>
    public class TextBox : HtmlGenericControl
    {

        /// <summary>
        /// Gets or sets the text in the control.
        /// </summary>
        public string Text
        {
            get { return Convert.ToString(GetValue(TextProperty)); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly RedwoodProperty TextProperty =
            RedwoodProperty.Register<string, TextBox>(t => t.Text, "");


        /// <summary>
        /// Gets or sets the mode of the text field.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public TextBoxType Type
        {
            get { return (TextBoxType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly RedwoodProperty TypeProperty =
            RedwoodProperty.Register<TextBoxType, TextBox>(c => c.Type, TextBoxType.Normal);


        /// <summary>
        /// Gets or sets whether the KO Text property binding should render with valueUpdate:'afterkeydown'
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool UpdateTextAfterKeydown
        {
            get { return (bool)GetValue(UpdateTextAfterKeydownProperty); }
            set { SetValue(UpdateTextAfterKeydownProperty, value); }
        }
        public static readonly RedwoodProperty UpdateTextAfterKeydownProperty
            = RedwoodProperty.Register<bool, TextBox>(c => c.UpdateTextAfterKeydown, false);


        /// <summary>
        /// Gets or sets the command that will be triggered when the control text is changed.
        /// </summary>
        public Action Changed
        {
            get { return (Action)GetValue(ChangedProperty); }
            set { SetValue(ChangedProperty, value); }
        }
        public static readonly RedwoodProperty ChangedProperty =
            RedwoodProperty.Register<Action, CheckableControlBase>(t => t.Changed, null);




        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("value", this, TextProperty, () =>
            {
                if (Type != TextBoxType.MultiLine)
                {
                    writer.AddAttribute("value", "Text");
                }
            }, UpdateTextAfterKeydown ? "afterkeydown" : null);

            if (Type == TextBoxType.Normal)
            {
                TagName = "input";
            }
            else if (Type == TextBoxType.Password)
            {
                writer.AddAttribute("type", "password");
                TagName = "input";
            }
            else if (Type == TextBoxType.MultiLine)
            {
                TagName = "textarea";
            }

            // prepare changed event attribute
            var changedBinding = GetCommandBinding(ChangedProperty);
            if (changedBinding != null)
            {
                writer.AddAttribute("onchange", KnockoutHelper.GenerateClientPostBackScript(changedBinding, context, this, true));
            }

            base.AddAttributesToRender(writer, context);
        }


        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            if (Type == TextBoxType.MultiLine && GetValueBinding(TextProperty) == null)
            {
                writer.WriteText(Text);
            }
        }
    }
}
