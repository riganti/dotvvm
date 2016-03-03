using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Container for content that will be displayed for the time the page is doing a postback.
    /// </summary>
    public class UpdateProgress : HtmlGenericControl
    {
        public UpdateProgress() : base("div")
        {
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddKnockoutDataBind("dotvvm-UpdateProgress-Visible", "true");

            base.AddAttributesToRender(writer, context);
        }
    }
}
