using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.Controls
{
    public class AttributeToStringConversionControl : HtmlGenericControl
    {
        public double Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DotvvmProperty ValueProperty
            = DotvvmProperty.Register<double, AttributeToStringConversionControl>(c => c.Value, 0);

        public AttributeToStringConversionControl() : base("input")
        {
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddAttribute("type", "range");
            writer.AddAttribute("min", "1");
            writer.AddAttribute("max", "100");
            writer.AddAttribute("step", "0.1");

            base.AddAttributesToRender(writer, context);
        }

        protected override void OnPreRender(IDotvvmRequestContext context)
        {
            Attributes.Set("value", GetValueRaw(ValueProperty));
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.RenderSelfClosingTag("input");
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
        }

    }
}
