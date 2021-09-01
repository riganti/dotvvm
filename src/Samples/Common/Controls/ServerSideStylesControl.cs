using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class ServerSideStylesControl : HtmlGenericControl
    {
        public string Added
        {
            get { return Convert.ToString(GetValue(CustomProperty)); }
            set { SetValue(CustomProperty, value); }
        }

        public static readonly DotvvmProperty AddedProperty =
            DotvvmProperty.Register<string, ServerSideStylesControl>(t => t.Added, "");

        public string Custom
        {
            get { return Convert.ToString(GetValue(CustomProperty)); }
            set { SetValue(CustomProperty, value); }
        }

        [MarkupOptions(AllowBinding = true)]
        public static readonly DotvvmProperty CustomProperty =
            DotvvmProperty.Register<string, ServerSideStylesControl>(t => t.Custom, "");

        public ServerSideStylesControl() : base("input")
        {
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddAttribute("type", "button");

            AddCustomAttributes(writer);
            base.AddAttributesToRender(writer, context);
        }

        private void AddCustomAttributes(IHtmlWriter writer)
        {
            var customBinding = GetValueBinding(CustomProperty);
            if (customBinding != null)
            {
                writer.AddKnockoutDataBind("customAttr", this, CustomProperty);
            }
            else
            {
                // render the value in the HTML
                writer.AddAttribute("customAttr", Custom);
            }
        }
    }
}
