using System;

namespace Redwood.Framework.Controls
{
    [ControlMarkupOptions(AllowContent = true)]
    public class Panel : HtmlGenericControl
    {
        public Panel()
        {
            this.TagName = "div";
        }
    }
}
