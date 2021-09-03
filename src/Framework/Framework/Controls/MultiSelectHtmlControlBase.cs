using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a multi-select HTML element control.
    /// </summary>
    public abstract class MultiSelectHtmlControlBase : MultiSelector
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectHtmlControlBase"/> class.
        /// </summary>
        public MultiSelectHtmlControlBase() : base("select")
        {
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RenderMultipleAttribute(writer);
            RenderEnabledProperty(writer);
            RenderOptionsProperties(writer);
            RenderSelectedValueProperty(writer);

            base.AddAttributesToRender(writer, context);

            RenderChangedEvent(writer);
        }

        protected virtual void RenderMultipleAttribute(IHtmlWriter writer)
        {
            writer.AddAttribute("multiple", "multiple");
        }

        protected virtual void RenderEnabledProperty(IHtmlWriter writer)
        {
            SelectHtmlControlHelpers.RenderEnabledProperty(writer, this);
        }

        protected virtual void RenderOptionsProperties(IHtmlWriter writer)
        {
            SelectHtmlControlHelpers.RenderOptionsProperties(writer, this);
        }

        protected virtual void RenderChangedEvent(IHtmlWriter writer)
        {
            SelectHtmlControlHelpers.RenderChangedEvent(writer, this);
        }

        protected virtual void RenderSelectedValueProperty(IHtmlWriter writer)
        {
            writer.AddKnockoutDataBind("selectedOptions", this, SelectedValuesProperty, renderEvenInServerRenderingMode: true);
        }
    }
}
