using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A container for a level of the <see cref="HierarchyRepeater"/> when rendering on the server.
    /// </summary>
    public class HierarchyRepeaterLevel : DotvvmControl
    {
        public IValueBinding? ForeachExpression { get; set; }

        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (ForeachExpression is null)
            {
                base.RenderControl(writer, context);
                return;
            }

            writer.AddKnockoutDataBind("dotvvm-SSR-foreach", new KnockoutBindingGroup
            {
                { "data", ForeachExpression.GetKnockoutBindingExpression(this)}
            });
            base.RenderControl(writer, context);
        }
    }
}
