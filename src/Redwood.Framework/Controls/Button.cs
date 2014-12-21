using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    public class Button : HtmlGenericControl
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


        public Button() : base("input")
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

            writer.AddAttribute("type", "button");
            writer.AddAttribute("value", Text);
            base.Render(writer, context);
        }
    }
}
