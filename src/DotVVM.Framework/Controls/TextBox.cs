using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a HTML text input control.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class TextBox : HtmlGenericControl
    {
        private bool isFormattingRequired;

        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled and can be modified.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        public static readonly DotvvmProperty EnabledProperty =
            DotvvmPropertyWithFallback.Register<bool, TextBox>(nameof(Enabled), FormControls.EnabledProperty);


        /// <summary>
        /// Gets or sets a format of presentation of value to client.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        public static readonly DotvvmProperty FormatStringProperty =
            DotvvmProperty.Register<string, TextBox>(t => t.FormatString);

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
        /// Gets or sets whether all text inside the TextBox becomes selected when the element gets focused.
        /// </summary>
        public bool SelectAllOnFocus
        {
            get { return (bool)GetValue(SelectAllOnFocusProperty); }
            set { SetValue(SelectAllOnFocusProperty, value); }
        }

        public static readonly DotvvmProperty SelectAllOnFocusProperty =
            DotvvmProperty.Register<bool, TextBox>(t => t.SelectAllOnFocus, false);

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
        /// Gets or sets whether the viewmodel property will be updated after the key is pressed. 
        /// By default, the viewmodel is updated after the control loses its focus.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool UpdateTextAfterKeydown
        {
            get { return (bool)GetValue(UpdateTextAfterKeydownProperty); }
            set { SetValue(UpdateTextAfterKeydownProperty, value); }
        }

        public static readonly DotvvmProperty UpdateTextAfterKeydownProperty =
            DotvvmProperty.Register<bool, TextBox>(c => c.UpdateTextAfterKeydown, false);

        /// <summary>
        /// Gets or sets the type of value being formatted - Number or DateTime.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [Obsolete("ValueType property is no longer required, it is automatically inferred from compile-time type of Text binding")]
        public FormatValueType ValueType
        {
            get { return (FormatValueType)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        [Obsolete("ValueType property is no longer required, it is automatically inferred from compile-time type of Text binding")]
        public static readonly DotvvmProperty ValueTypeProperty =
            DotvvmProperty.Register<FormatValueType, TextBox>(t => t.ValueType);

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            isFormattingRequired = !string.IsNullOrEmpty(FormatString) ||
                #pragma warning disable
                ValueType != FormatValueType.Text ||
                #pragma warning restore
                Literal.NeedsFormatting(GetValueBinding(TextProperty));
            if (isFormattingRequired)
            {
                context.ResourceManager.AddCurrentCultureGlobalizationResource();
            }

            base.OnPreRender(context);
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            AddEnabledPropertyToRender(writer);
            AddTypeAttributeToRender(writer);
            AddChangedPropertyToRender(writer, context);
            AddSelectAllOnFocusPropertyToRender(writer, context);
            if (isFormattingRequired)
            {
                AddFormatBindingToRender(writer, context);
            }
            else
            {
                AddValueBindingToRender(writer, context);
            }
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (Type == TextBoxType.MultiLine && GetValueBinding(TextProperty) == null)
            {
                writer.WriteText(Text);
            }
        }

        private void AddEnabledPropertyToRender(IHtmlWriter writer)
        {
            writer.AddKnockoutDataBind("enable", this, EnabledProperty, () =>
            {
                if (!Enabled)
                {
                    writer.AddAttribute("disabled", "disabled");
                }
            });
        }

        private void AddFormatBindingToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // if format is set then use different value binding  which supports the format
            writer.AddKnockoutDataBind("dotvvm-textbox-text", this, TextProperty, () =>
            {
                if (Type != TextBoxType.MultiLine)
                {
                    writer.AddAttribute("value", Text);
                }
            }, UpdateTextAfterKeydown ? "afterkeydown" : null, renderEvenInServerRenderingMode: true);
            var binding = GetValueBinding(TextProperty);
            var resultType = binding?.ResultType;
            var formatString = FormatString;
            if (string.IsNullOrEmpty(formatString))
            {
                if (Type == TextBoxType.Date)
                    formatString = "yyyy-MM-dd";
                else
                    formatString = "G";
            }

            writer.AddAttribute("data-dotvvm-format", formatString);

            #pragma warning disable
            if (ValueType != FormatValueType.Text)
            {
                writer.AddAttribute("data-dotvvm-value-type", ValueType.ToString().ToLowerInvariant());
            }
            #pragma warning restore
            else if (resultType == typeof(DateTime) || resultType == typeof(DateTime?))
            {
                writer.AddAttribute("data-dotvvm-value-type", "datetime");
            }
            else if (resultType.IsNumericType())
            {
                writer.AddAttribute("data-dotvvm-value-type", "number");
            }
        }

        private void AddChangedPropertyToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // prepare changed event attribute
            var changedBinding = GetCommandBinding(ChangedProperty);

            base.AddAttributesToRender(writer, context);

            if (changedBinding != null)
            {
                writer.AddAttribute("onchange", KnockoutHelper.GenerateClientPostBackScript(nameof(Changed), 
                    changedBinding, this, useWindowSetTimeout: true, isOnChange: true), true, ";");
            }
        }

        private void AddSelectAllOnFocusPropertyToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            const string KoBindingName = "dotvvm-textbox-select-all-on-focus";
            writer.AddKnockoutDataBind(KoBindingName, this, SelectAllOnFocusProperty, () =>
            {
                writer.AddKnockoutDataBind(KoBindingName, this.GetKnockoutBindingExpression(SelectAllOnFocusProperty));
            }, renderEvenInServerRenderingMode: true);
        }

        private void AddTypeAttributeToRender(IHtmlWriter writer)
        {
            var isTagName = Type.TryGetTagName(out string tagName);
            var isInputType = Type.TryGetInputType(out string inputType);

            if (isTagName)
            {
                TagName = tagName;
                // do not overwrite type attribute
                if (Type == TextBoxType.Normal && !Attributes.ContainsKey("type"))
                {
                    writer.AddAttribute("type", "text");
                }
                return;
            }

            if (isInputType)
            {
                writer.AddAttribute("type", inputType);
                TagName = "input";
                return;
            }

            throw new NotSupportedException($"TextBox Type '{ Type }' not supported");
        }

        private void AddValueBindingToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // use standard value binding
            writer.AddKnockoutDataBind("value", this, TextProperty, () =>
            {
                if (Type != TextBoxType.MultiLine)
                {
                    writer.AddAttribute("value", Text);
                }
            }, UpdateTextAfterKeydown ? "afterkeydown" : null, renderEvenInServerRenderingMode: true);
        }
    }
}