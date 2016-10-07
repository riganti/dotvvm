using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class ServerRenderedLabel : HtmlGenericControl
    {

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<string, ServerRenderedLabel>(c => c.Text, "");


        public ServerRenderedLabel()
        {
            TagName = "span";
        }


        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.WriteUnencodedText(Text);
        }
    }
}