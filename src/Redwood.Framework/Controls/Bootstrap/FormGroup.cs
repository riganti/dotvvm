using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls.Bootstrap
{
    public class HorizontalFormGroup : HtmlGenericControl
    {

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }
        public static readonly RedwoodProperty LabelTextProperty =
            RedwoodProperty.Register<string, HorizontalFormGroup>(t => t.LabelText, "");


        public HorizontalFormGroup() : base("div")
        {
        }

        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            writer.AddAttribute("class", "form-group");
            base.Render(writer, context);
        }

        protected override void RenderChildren(IHtmlWriter writer, RenderContext context)
        {
            RenderLabel(writer);
            
            writer.AddAttribute("class", "col-sm-10");
            writer.RenderBeginTag("div");
            base.RenderChildren(writer, context);
            writer.RenderEndTag();
        }

        private void RenderLabel(IHtmlWriter writer)
        {
            writer.AddAttribute("class", "control-label col-sm-2");
            var textBinding = GetBinding(LabelTextProperty);
            if (textBinding != null)
            {
                writer.AddKnockoutDataBind("text", textBinding as ValueBindingExpression, this, LabelTextProperty);
                writer.RenderBeginTag("label");
                writer.RenderEndTag();
            }
            else
            {
                writer.RenderBeginTag("label");
                writer.WriteText(LabelText);
                writer.RenderEndTag();
            }
        }
    }
}
