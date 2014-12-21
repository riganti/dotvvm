using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    public class LinkButton : HtmlGenericControl
    {

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly RedwoodProperty TextProperty =
            RedwoodProperty.Register<string, Button>(t => t.Text, "");


        public Action Click
        {
            get { return (Action)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }
        public static readonly RedwoodProperty ClickProperty =
            RedwoodProperty.Register<Action, Button>(t => t.Click, null);


        public LinkButton()
            : base("a")
        {
        }

        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            var clickBinding = GetBinding(ClickProperty);
            if (clickBinding != null)
            {
                EnsureControlHasId();
                writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript(clickBinding as CommandBindingExpression, context, ID));
            }

            var textBinding = GetBinding(TextProperty);
            if (textBinding != null)
            {
                writer.AddKnockoutDataBind("text", textBinding as ValueBindingExpression);
            }
            writer.AddAttribute("href", "#");

            base.Render(writer, context);
        }

        protected override void RenderChildren(IHtmlWriter writer, RenderContext context)
        {
            var textBinding = GetBinding(TextProperty);
            if (textBinding == null && !string.IsNullOrEmpty(Text))
            {
                // render Text property
                writer.WriteText(Text);
            }
            else
            {
                // render control contents
                base.RenderChildren(writer, context);
            }
        }
    }
}
