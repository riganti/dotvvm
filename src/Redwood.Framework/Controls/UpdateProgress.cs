using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    public class UpdateProgress : HtmlGenericControl
    {
        public UpdateProgress() : base("div")
        {
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("redwoodUpdateProgressVisible", "true");

            base.AddAttributesToRender(writer, context);
        }
    }
}
