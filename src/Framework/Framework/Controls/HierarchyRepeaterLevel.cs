using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A container for a level of the <see cref="HierarchyRepeater"/>.
    /// </summary>
    public class HierarchyRepeaterLevel : DotvvmControl
    {
        public string? ItemTemplateId { get; set; }

        public IValueBinding? ForeachExpression { get; set; }

        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (ForeachExpression is null)
            {
                base.RenderControl(writer, context);
                return;
            }

            if (!RenderOnServer)
                writer.AddKnockoutDataBind("template", new KnockoutBindingGroup {
                    { "foreach", ForeachExpression.GetKnockoutBindingExpression(this) },
                    { "name", ItemTemplateId ?? string.Empty, true }
                });
            else
                writer.AddKnockoutDataBind("dotvvm-SSR-foreach", new KnockoutBindingGroup {
                    { "data", ForeachExpression.GetKnockoutBindingExpression(this)}
                });
            base.RenderControl(writer, context);
        }
    }
}
