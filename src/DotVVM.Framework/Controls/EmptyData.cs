using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Content of this control is displayed iff DataSource is empty or null
    /// </summary>
    public class EmptyData : ItemsControl
    {
        /// <summary>
        /// Gets or sets the name of the tag that wraps the Repeater.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string WrapperTagName
        {
            get { return (string)GetValue(WrapperTagNameProperty); }
            set { SetValue(WrapperTagNameProperty, value); }
        }
        public static readonly DotvvmProperty WrapperTagNameProperty =
            DotvvmProperty.Register<string, EmptyData>(t => t.WrapperTagName, "div");

        /// <summary>
        /// Gets or sets whether the control should render a wrapper element.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool RenderWrapperTag
        {
            get { return (bool)GetValue(RenderWrapperTagProperty); }
            set { SetValue(RenderWrapperTagProperty, value); }
        }
        public static readonly DotvvmProperty RenderWrapperTagProperty =
            DotvvmProperty.Register<bool, EmptyData>(t => t.RenderWrapperTag, true);

        protected override bool RendersHtmlTag => RenderWrapperTag;

        public EmptyData()
        {
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (!RenderOnServer)
            {
                if (RenderWrapperTag)
                    writer.AddKnockoutDataBind("visible", $"!({ GetForeachDataBindJavascriptExpression() }).length");
                else
                    writer.WriteKnockoutDataBindComment("visible", $"!({ GetForeachDataBindJavascriptExpression() }).length");

                if (DataSource != null && RenderWrapperTag && GetIEnumerableFromDataSource(DataSource).OfType<object>().Any())
                {
                    writer.AddStyleAttribute("display", "none");
                }
            }

            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (!RenderWrapperTag && !RenderOnServer)
                writer.WriteKnockoutDataBindEndComment();

            base.RenderEndTag(writer, context);
        }

        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            TagName = WrapperTagName;
            // if RenderOnServer && DataSource is not empty then don't render anything
            if (!RenderOnServer || GetIEnumerableFromDataSource(DataSource)?.GetEnumerator()?.MoveNext() != true)
            {
                base.RenderControl(writer, context);
            }
        }
    }
}
