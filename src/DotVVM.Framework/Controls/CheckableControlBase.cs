using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A base control for checkbox and radiobutton controls.
    /// </summary>
    public abstract class CheckableControlBase : HtmlGenericControl
    {
        private bool isLabelRequired;

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
        /// Gets or sets the value that will be used as a result when the control is checked.
        /// Use this property in combination with the CheckedItem or CheckedItems property.
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
        public Command Changed
        {
            get { return (Command)GetValue(ChangedProperty); }
            set { SetValue(ChangedProperty, value); }
        }

        public static readonly DotvvmProperty ChangedProperty =
            DotvvmProperty.Register<Command, CheckableControlBase>(t => t.Changed, null);

        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled and can be clicked on.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        public static readonly DotvvmProperty EnabledProperty =
            DotvvmPropertyWithFallback.Register<bool, CheckableControlBase>(nameof(Enabled), FormControls.EnabledProperty);

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckableControlBase"/> class.
        /// </summary>
        public CheckableControlBase() : base("span")
        {

        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            base.OnPreRender(context);

            isLabelRequired = HasValueBinding(TextProperty) || !string.IsNullOrEmpty(Text) || !HasOnlyWhiteSpaceContent();
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (isLabelRequired)
            {
                writer.RenderBeginTag("label");
            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // label end tag
            if (isLabelRequired)
            {
                writer.RenderEndTag();
            }
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            AddAttributesToInput(writer);
            RenderInputTag(writer);

            if (isLabelRequired)
            {
                if (HasValueBinding(TextProperty))
                {
                    writer.AddKnockoutDataBind("text", GetValueBinding(TextProperty).GetKnockoutBindingExpression(this));
                    writer.RenderBeginTag(TagName);
                    writer.RenderEndTag();
                }
                else if (!string.IsNullOrEmpty(Text))
                {
                    writer.RenderBeginTag(TagName);
                    writer.WriteText(Text);
                    writer.RenderEndTag();
                }
                else if (!HasOnlyWhiteSpaceContent())
                {
                    writer.RenderBeginTag(TagName);
                    RenderChildren(writer, context);
                    writer.RenderEndTag();
                }
            }
        }

        protected virtual void AddAttributesToInput(IHtmlWriter writer)
        {
            // prepare changed event attribute
            var changedBinding = GetCommandBinding(ChangedProperty);
            if (changedBinding != null)
            {
                writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript(nameof(Changed), changedBinding, this, useWindowSetTimeout: true, returnValue: true, isOnChange: true));
            }

            // handle enabled attribute
            writer.AddKnockoutDataBind("enable", this, EnabledProperty, () =>
            {
                if (!Enabled)
                {
                    writer.AddAttribute("disabled", "disabled");
                }
            });
        }

        /// <summary>
        /// Renders the input tag.
        /// </summary>
        protected abstract void RenderInputTag(IHtmlWriter writer);
    }
}