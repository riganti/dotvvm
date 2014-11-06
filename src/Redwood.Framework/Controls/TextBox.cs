using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Utils;

namespace Redwood.Framework.Controls
{
    public class TextBox : HtmlGenericControl
    {

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly RedwoodProperty TextProperty =
            RedwoodProperty.Register<string, TextBox>(t => t.Text, "");

        public TextBox() : base("input")
        {
            
        }


        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            var textBinding = GetBinding(TextProperty);
            if (textBinding != null)
            {
                writer.AddKnockoutDataBind("value", textBinding as ValueBindingExpression);
            }
            else
            {
                writer.AddAttribute("value", Text);
            }

            writer.AddAttribute("type", "text");
            base.Render(writer, context);
        }
    }
}
