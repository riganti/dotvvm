using System;

namespace DotVVM.Framework.Controls
{
    [ControlMarkupOptions(AllowContent = true)]
    public class Panel : HtmlGenericControl
    {
        public Panel() : base("div")
        {
        }
    }
}
