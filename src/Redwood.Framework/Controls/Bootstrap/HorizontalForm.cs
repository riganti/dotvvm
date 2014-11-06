using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls.Bootstrap
{
    public class HorizontalForm : HtmlGenericControl
    {

        public HorizontalForm() : base("form")
        {
            
        }

        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            writer.AddAttribute("class", "form-horizontal");
            writer.AddAttribute("role", "form");

            base.Render(writer, context);
        }
    }
}
