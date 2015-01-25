using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;
using Redwood.Framework.Runtime;

namespace Redwood.Samples.BasicSamples.Controls
{
    public class ServerRenderedLabel : HtmlGenericControl
    {

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly RedwoodProperty TextProperty =
            RedwoodProperty.Register<string, ServerRenderedLabel>(c => c.Text, "");


        public ServerRenderedLabel()
        {
            TagName = "span";
        }


        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            writer.WriteUnencodedText(Text);
        }
    }
}