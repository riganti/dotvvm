using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Parser;
using Redwood.Framework.Runtime;

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
            ResourceDependencies.Add(Constants.BootstrapResourceName);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddAttribute("class", "form-group");

            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            RenderLabel(writer);
            
            writer.AddAttribute("class", "col-sm-10");
            writer.RenderBeginTag("div");
            
            RenderChildren(writer, context);

            writer.RenderEndTag();
        }

        private void RenderLabel(IHtmlWriter writer)
        {
            writer.AddAttribute("class", "control-label col-sm-2");
            var textBinding = GetBinding(LabelTextProperty);
            if (textBinding != null)
            {
                writer.AddKnockoutDataBind("text", this, LabelTextProperty, () => { });
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
