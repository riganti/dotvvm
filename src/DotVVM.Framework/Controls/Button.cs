using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the HTML button which is able to trigger a postback.
    /// </summary>
    public class Button : ButtonBase
    {
        /// <summary>
        /// Gets or sets whether the control should render a submit button or a normal button (type="submit" or type="button").
        /// The submit button has some special features in the browsers, e.g. handles the Return key in HTML forms etc.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool IsSubmitButton
        {
            get { return (bool)GetValue(IsSubmitButtonProperty); }
            set { SetValue(IsSubmitButtonProperty, value); }
        }

        public static readonly DotvvmProperty IsSubmitButtonProperty
            = DotvvmProperty.Register<bool, Button>(c => c.IsSubmitButton, false);

        /// <summary>
        /// Gets or sets whether the control should render the &lt;input&gt; or the &lt;button&gt; tag in the HTML.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public ButtonTagName ButtonTagName
        {
            get { return (ButtonTagName)GetValue(ButtonTagNameProperty); }
            set { SetValue(ButtonTagNameProperty, value); }
        }

        public static readonly DotvvmProperty ButtonTagNameProperty
            = DotvvmProperty.Register<ButtonTagName, Button>(c => c.ButtonTagName, ButtonTagName.input);

        /// <summary>
        /// Initializes a new instance of the <see cref="Button"/> class.
        /// </summary>
        public Button() : base("input")
        {
            if (ButtonTagName == ButtonTagName.button)
            {
                TagName = "button";
            }
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if ((HasBinding(TextProperty) || !string.IsNullOrEmpty(Text)) && !HasOnlyWhiteSpaceContent())
            {
                throw new DotvvmControlException(this, "Text property and inner content of the <dot:Button> control cannot be set at the same time!");
            }

            if (ButtonTagName == ButtonTagName.button)
            {
                TagName = "button";
            }
            writer.AddAttribute("type", IsSubmitButton ? "submit" : "button");

            var clickBinding = GetCommandBinding(ClickProperty);
            if (clickBinding != null)
            {
                writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript(nameof(Click), clickBinding, this));
            }

            writer.AddKnockoutDataBind(ButtonTagName == ButtonTagName.input ? "value" : "text", this, TextProperty, () =>
            {
                if (!HasOnlyWhiteSpaceContent())
                {
                    if (ButtonTagName == ButtonTagName.input)
                    {
                        // if there is only a text content, extract it into the Text property; if there is HTML, we don't support it
                        string textContent;
                        if (!TryGetTextContent(out textContent))
                        {
                            throw new DotvvmControlException(this, "The <dot:Button> control cannot have inner HTML connect unless the 'ButtonTagName' property is set to 'button'!");
                        }
                        Text = textContent;
                    }
                }

                if (ButtonTagName == ButtonTagName.input)
                {
                    writer.AddAttribute("value", Text);
                }
            });

            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (ButtonTagName == ButtonTagName.input)
            {
                writer.RenderSelfClosingTag(ButtonTagName.ToString());
            }
            else
            {
                base.RenderBeginTag(writer, context);
            }
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (ButtonTagName == ButtonTagName.button)
            {
                if (!HasValueBinding(TextProperty))
                {
                    // render contents inside
                    if (IsPropertySet(TextProperty))
                    {
                        if (HasOnlyWhiteSpaceContent())
                        {
                            throw new DotvvmControlException(this, "Text property and inner content of the <dot:Button> control cannot be set at the same time!");
                        }
                        writer.WriteText(Text);
                    }
                    else
                    {
                        base.RenderContents(writer, context);
                    }
                }
            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (ButtonTagName != ButtonTagName.input)
            {
                base.RenderEndTag(writer, context);
            }
        }

        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            if(control.Properties.ContainsKey(TextProperty) && control.Content.Any(n => n.DothtmlNode.IsNotEmpty()))
            {
                yield return new ControlUsageError("Text property and inner content of the <dot:Button> control cannot be set at the same time!", control.DothtmlNode);
            }
            //ResolvedPropertySetter buttonType;
            //bool allowcontent = false;
            //if(control.Properties.TryGetValue(ButtonTagNameProperty, out buttonType))
            //{
            //    allowcontent = ButtonTagName.button.Equals((buttonType as ResolvedPropertyValue)?.Value);
            //}
            //if (!allowcontent && control.Content.Any(n => n.DothtmlNode.IsNotEmpty())) yield return new ControlUsageError("The <dot:Button> control cannot have inner HTML connect unless the 'ButtonTagName' property is set to 'button'!", control.DothtmlNode);
        }
    }
}