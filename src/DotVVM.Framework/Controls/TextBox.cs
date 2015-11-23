using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a HTML text input control.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class TextBox : HtmlGenericControl
    {

        /// <summary>
        /// Gets or sets the text in the control.
        /// </summary>
        [MarkupOptions(Required = true)]
        public string Text
        {
            get { return Convert.ToString(GetValue(TextProperty)); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<string, TextBox>(t => t.Text, "");



        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled and can be modified.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty =
            DotvvmProperty.Register<bool, TextBox>(t => t.Enabled, true);




        /// <summary>
        /// Gets or sets the mode of the text field.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public TextBoxType Type
        {
            get { return (TextBoxType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        public static readonly DotvvmProperty TypeProperty =
            DotvvmProperty.Register<TextBoxType, TextBox>(c => c.Type, TextBoxType.Normal);


        /// <summary>
        /// Gets or sets whether the viewmodel property will be updated after the key is pressed. By default, the viewmodel is updated after the control loses its focus.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool UpdateTextAfterKeydown
        {
            get { return (bool)GetValue(UpdateTextAfterKeydownProperty); }
            set { SetValue(UpdateTextAfterKeydownProperty, value); }
        }
        public static readonly DotvvmProperty UpdateTextAfterKeydownProperty
            = DotvvmProperty.Register<bool, TextBox>(c => c.UpdateTextAfterKeydown, false);


        /// <summary>
        /// Gets or sets the command that will be triggered when the control text is changed.
        /// </summary>
        public Command Changed
        {
            get { return (Command)GetValue(ChangedProperty); }
            set { SetValue(ChangedProperty, value); }
        }
        public static readonly DotvvmProperty ChangedProperty =
            DotvvmProperty.Register<Command, TextBox>(t => t.Changed, null);




        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("enable", this, EnabledProperty, () =>
            {
                if (!Enabled)
                {
                    writer.AddAttribute("disabled", "disabled");
                }
            });

            writer.AddKnockoutDataBind("value", this, TextProperty, () =>
            {
                if (Type != TextBoxType.MultiLine)
                {
                    writer.AddAttribute("value", Text);
                }
            }, UpdateTextAfterKeydown ? "afterkeydown" : null, renderEvenInServerRenderingMode: true);

            if (Type == TextBoxType.MultiLine)
            {
                TagName = "textarea";
            }
            else if (Type == TextBoxType.Normal)
            {
                TagName = "input";
                // do not overwrite type attribute
                if (!Attributes.ContainsKey("type"))
                {
                    writer.AddAttribute("type", "text");
                }
            }
            else
            {
                string type = null;
                switch (Type)
                {
                    case TextBoxType.Password:
                        type = "password";
                        break;
                    case TextBoxType.Telephone:
                        type = "tel";
                        break;
                    case TextBoxType.Url:
                        type = "url";
                        break;
                    case TextBoxType.Email:
                        type = "email";
                        break;
                    case TextBoxType.Date:
                        type = "date";
                        break;
                    case TextBoxType.Time:
                        type = "time";
                        break;
                    case TextBoxType.Color:
                        type = "color";
                        break;
                    case TextBoxType.Search:
                        type = "search";
                        break;
                    default:
                        throw new NotSupportedException($"TextBox Type '{ Type }' not supported");
                }
                writer.AddAttribute("type", type);
                TagName = "input";
            }

            // prepare changed event attribute
            var changedBinding = GetCommandBinding(ChangedProperty);
            if (changedBinding != null)
            {
                writer.AddAttribute("onchange", KnockoutHelper.GenerateClientPostBackScript(nameof(Changed), changedBinding, context, this, true, isOnChange: true));
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
