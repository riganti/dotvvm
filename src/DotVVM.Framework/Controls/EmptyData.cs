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
    /// Content of this control is displayed iff DataSource is empty or null
    /// </summary>
    public class EmptyData : ItemsControl
    {
        public EmptyData() : base("div")
        {
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (!RenderOnServer)
            {
                writer.AddKnockoutDataBind("visible", $"!({ GetForeachDataBindJavascriptExpression() }).length");

                if (DataSource != null && GetIEnumerableFromDataSource(DataSource).OfType<object>().Any())
                {
                    writer.AddStyleAttribute("display", "none");
                }
            }

            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (!RenderOnServer || GetIEnumerableFromDataSource(DataSource)?.GetEnumerator()?.MoveNext() != true)
            {
                base.RenderControl(writer, context);
            }
        }
    }
}
