using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls.Bootstrap
{
    public class HorizontalForm : HtmlGenericControl
    {

        public HorizontalForm() : base("form")
        {
            ResourceDependencies.Add(Constants.BootstrapResourceName);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddAttribute("class", "form-horizontal");
            writer.AddAttribute("role", "form");

            base.AddAttributesToRender(writer, context);
        }
    }
}
