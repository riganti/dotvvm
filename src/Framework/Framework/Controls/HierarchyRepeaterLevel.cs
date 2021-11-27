using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A container for a level of the <see cref="HierarchyRepeater"/>.
    /// </summary>
    public class HierarchyRepeaterLevel : DotvvmControl
    {
        public string? ItemTemplateId { get; set; }

        public string? ForeachExpression { get; set; }

        public bool IsRoot { get; set; } = false;

        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (ForeachExpression is null)
            {
                base.RenderControl(writer, context);
                return;
            }

            if (!RenderOnServer)
            {
                var koGroup = new KnockoutBindingGroup {
                    { "foreach", ForeachExpression }
                };
                koGroup.AddValue("name", ItemTemplateId.NotNull());
                koGroup.AddValue("hierarchyRole", IsRoot ? "Root" : "Child");
                writer.WriteKnockoutDataBindComment("template", koGroup.ToString());
            }
            else
            {
                writer.WriteKnockoutDataBindComment("dotvvm-SSR-foreach", new KnockoutBindingGroup {
                    { "data", ForeachExpression}
                }.ToString());
            }

            base.RenderControl(writer, context);

            writer.WriteKnockoutDataBindEndComment();
        }
    }
}
