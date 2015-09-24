using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a HTML button.
    /// </summary>
    public class Button : ButtonBase
    {

        /// <summary>
        /// Gets or sets whether the button should render as input[type=submit] or input[type=button]. 
        /// The submit button has some special features, e.g. handles the Return key in HTML forms etc.
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
        /// Controls which tag would be used to render button.
        /// (input or button)
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
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            if ((HasBinding(TextProperty) || !string.IsNullOrEmpty(Text)) && !HasOnlyWhiteSpaceContent())
            {
                throw new Exception("The <dot:Button> control cannot have both inner content and the Text property set!");     // TODO
            }

            if (ButtonTagName == ButtonTagName.button)
            {
                TagName = "button";
            }
            writer.AddAttribute("type", IsSubmitButton ? "submit" : "button");

            var clickBinding = GetBinding(ClickProperty);
            if (clickBinding is CommandBindingExpression)
            {
                writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript((CommandBindingExpression)clickBinding, context, this));
            }
            
            writer.AddKnockoutDataBind(ButtonTagName == ButtonTagName.input ? "value" : "text", this, TextProperty, () =>
            {
                if (!HasOnlyWhiteSpaceContent())
                {
                    if (ButtonTagName == ButtonTagName.input)
                    {
                        throw new Exception("The <dot:Button> control cannot have inner content unless the 'ButtonTagName' property is set to 'button'!");     // TODO
                    }
                }

                if (ButtonTagName == ButtonTagName.input)
                {
                    writer.AddAttribute("value", Text);
                }
            });

            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderBeginTag(IHtmlWriter writer, RenderContext context)
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

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            if (ButtonTagName == ButtonTagName.button)
            {
                if (!HasBinding(TextProperty))
                {
                    // render contents inside
                    if (!HasOnlyWhiteSpaceContent())
                    {
                        base.RenderContents(writer, context);
                    }
                    else
                    {
                        writer.WriteText(Text);    
                    }
                }
            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, RenderContext context)
        {
            if (ButtonTagName != ButtonTagName.input)
            {
                base.RenderEndTag(writer, context);
            }
        }
    }
}
