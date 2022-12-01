using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a HTML text input control.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class TextBox : HtmlGenericControl
    {
        private FormatValueType resolvedValueType;
        private string? implicitFormatString;

        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled and can be modified.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty)!; }
            set { SetValue(EnabledProperty, value); }
        }

        public static readonly DotvvmProperty EnabledProperty =
            DotvvmPropertyWithFallback.Register<bool, TextBox>(nameof(Enabled), FormControls.EnabledProperty);


        /// <summary>
        /// Gets or sets a format of presentation of value to client.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string? FormatString
        {
            get { return (string?)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        public static readonly DotvvmProperty FormatStringProperty =
            DotvvmProperty.Register<string?, TextBox>(t => t.FormatString);

        /// <summary>
        /// Gets or sets the command that will be triggered when the onchange event is fired.
        /// </summary>
        public Command? Changed
        {
            get { return (Command?)GetValue(ChangedProperty); }
            set { SetValue(ChangedProperty, value); }
        }

        public static readonly DotvvmProperty ChangedProperty =
            DotvvmProperty.Register<Command?, TextBox>(t => t.Changed, null);

        /// <summary>
        /// Gets or sets the command that will be triggered when the user is typing in the field.
        /// Be careful when using this event - triggering frequent postbacks can make bad user experience. Consider using static commands or a throttling postback handler.
        /// </summary>
        public Command? TextInput
        {
            get { return (Command?)GetValue(TextInputProperty); }
            set { SetValue(TextInputProperty, value); }
        }

        public static readonly DotvvmProperty TextInputProperty =
            DotvvmProperty.Register<Command?, TextBox>(t => t.TextInput, null);

        /// <summary>
        /// Gets or sets whether all text inside the TextBox becomes selected when the element gets focused.
        /// </summary>
        public bool SelectAllOnFocus
        {
            get { return (bool)GetValue(SelectAllOnFocusProperty)!; }
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
            get { return Convert.ToString(GetValue(TextProperty)).NotNull(); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<object, TextBox>(t => t.Text, "");

        /// <summary>
        /// Gets or sets the mode of the text field.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public TextBoxType Type
        {
            get { return (TextBoxType)GetValue(TypeProperty)!; }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DotvvmProperty TypeProperty =
            DotvvmProperty.Register<TextBoxType, TextBox>(c => c.Type, TextBoxType.Normal);

        /// <summary>
        /// Gets or sets whether the viewmodel property will be updated immediately after change. 
        /// By default, the viewmodel is updated after the control loses its focus.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool UpdateTextOnInput
        {
            get { return (bool)GetValue(UpdateTextOnInputProperty)!; }
            set { SetValue(UpdateTextOnInputProperty, value); }
        }
        public static readonly DotvvmProperty UpdateTextOnInputProperty =
            DotvvmProperty.Register<bool, TextBox>(
                nameof(UpdateTextOnInput),
                isValueInherited: true);

        public static FormatValueType ResolveValueType(IValueBinding? binding)
        {
            if (binding?.ResultType == typeof(DateTime) || binding?.ResultType == typeof(DateTime?))
            {
                return FormatValueType.DateTime;
            }
            else if (binding != null && (binding.ResultType == typeof(DateOnly) || binding.ResultType == typeof(DateOnly?)))
            {
                return FormatValueType.DateOnly;
            }
            else if (binding != null && (binding.ResultType == typeof(TimeOnly) || binding.ResultType == typeof(TimeOnly?)))
            {
                return FormatValueType.TimeOnly;
            }
            else if (binding != null && (binding.ResultType.IsNumericType() || Nullable.GetUnderlyingType(binding.ResultType)?.IsNumericType() == true))
            {
                return FormatValueType.Number;
            }
            return FormatValueType.Text;
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            var isTypeImplicitlyFormatted = Type.TryGetFormatString(out implicitFormatString);
            if (!string.IsNullOrEmpty(FormatString) && isTypeImplicitlyFormatted)
            {
                throw new NotSupportedException($"Property FormatString cannot be used with Type set to '{ Type }'." +
                    $" In this case browsers localize '{ Type }' themselves.");
            }

            if (!isTypeImplicitlyFormatted || implicitFormatString != null)
            {
                resolvedValueType = ResolveValueType(GetValueBinding(TextProperty));
            }

            if (resolvedValueType != FormatValueType.Text)
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
            AddBindingToRender(writer);
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
            switch (this.GetValueRaw(EnabledProperty))
            {
                case bool value:
                    if (!value)
                        writer.AddAttribute("disabled", "disabled");
                    break;
                case IValueBinding binding:
                    writer.AddKnockoutDataBind("enable", binding.GetKnockoutBindingExpression(this));
                    break;
                default:
                    if (!Enabled)
                        writer.AddAttribute("disabled", "disabled");
                    break;
            }
        }

        private void AddBindingToRender(IHtmlWriter writer)
        {
            // if format is set then use different value binding  which supports the format
            writer.AddKnockoutDataBind("dotvvm-textbox-text", this, TextProperty, () => {
                if (Type != TextBoxType.MultiLine)
                {
                    writer.AddAttribute("value", Text);
                }
            }, UpdateTextOnInput ? "input" : null, renderEvenInServerRenderingMode: true);

            if (resolvedValueType != FormatValueType.Text)
            {
                var formatString = FormatString;
                if (string.IsNullOrEmpty(formatString))
                {
                    formatString = implicitFormatString;
                }

                if (resolvedValueType == FormatValueType.DateTime)
                {
                    writer.AddAttribute("data-dotvvm-value-type", "datetime");
                }
                else if (resolvedValueType == FormatValueType.Number)
                {
                    writer.AddAttribute("data-dotvvm-value-type", "number");
                }
                else if (resolvedValueType == FormatValueType.DateOnly)
                {
                    writer.AddAttribute("data-dotvvm-value-type", "dateonly");
                    formatString ??= "D";
                }
                else if (resolvedValueType == FormatValueType.TimeOnly)
                {
                    writer.AddAttribute("data-dotvvm-value-type", "timeonly");
                    formatString ??= "T";
                }

                writer.AddAttribute("data-dotvvm-format", formatString ?? "G");
            }
        }

        private void AddChangedPropertyToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // prepare changed event attribute
            var changedBinding = GetCommandBinding(ChangedProperty);
            var textInputBinding = GetCommandBinding(TextInputProperty);

            base.AddAttributesToRender(writer, context);

            if (changedBinding != null)
            {
                writer.AddAttribute("onchange", KnockoutHelper.GenerateClientPostBackScript(nameof(Changed),
                    changedBinding, this, useWindowSetTimeout: true, isOnChange: true), true, ";");
            }
            if (textInputBinding != null)
            {
                writer.AddAttribute("oninput", KnockoutHelper.GenerateClientPostBackScript(nameof(TextInput),
                    textInputBinding, this, useWindowSetTimeout: true, isOnChange: true), true, ";");
            }
        }

        private void AddSelectAllOnFocusPropertyToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            const string KoBindingName = "dotvvm-textbox-select-all-on-focus";
            switch (this.GetValueRaw(SelectAllOnFocusProperty))
            {
                case false:
                    break;
                case IValueBinding valueBinding:
                    writer.AddKnockoutDataBind(KoBindingName, valueBinding.GetKnockoutBindingExpression(this));
                    break;
                case object _ when SelectAllOnFocus:
                    writer.AddKnockoutDataBind(KoBindingName, "true");
                    break;
            }
        }

        private void AddTypeAttributeToRender(IHtmlWriter writer)
        {
            var type = this.Type;

            if (type.TryGetTagName(out var tagName))
            {
                TagName = tagName;
                // do not overwrite type attribute
                if (type == TextBoxType.Normal && !Attributes.ContainsKey("type"))
                {
                    writer.AddAttribute("type", "text");
                }
                return;
            }

            if (type.TryGetInputType(out var inputType))
            {
                writer.AddAttribute("type", inputType);
                TagName = "input";
                return;
            }

            throw new NotSupportedException($"TextBox Type '{ type }' not supported");
        }
    }
}
