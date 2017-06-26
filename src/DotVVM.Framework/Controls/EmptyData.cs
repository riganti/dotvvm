using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Content of this control is displayed if and only if DataSource is empty or null
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
                    writer.AddKnockoutDataBind("visible", "!" + GetBinding(DataSourceProperty).GetProperty<DataSourceLengthBinding>().Binding.CastTo<IValueBinding>().GetKnockoutBindingExpression(this));
                else
                    writer.WriteKnockoutDataBindComment("visible", "!" + GetBinding(DataSourceProperty).GetProperty<DataSourceLengthBinding>().Binding.CastTo<IValueBinding>().GetKnockoutBindingExpression(this));

                if (DataSource != null && RenderWrapperTag && GetIEnumerableFromDataSource().OfType<object>().Any())
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
            if (!RenderOnServer || GetIEnumerableFromDataSource()?.GetEnumerator()?.MoveNext() != true)
            {
                base.RenderControl(writer, context);
            }
        }
    }
}