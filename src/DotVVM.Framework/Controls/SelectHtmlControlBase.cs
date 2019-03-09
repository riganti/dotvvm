using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders <c>select</c> HTML element control.
    /// </summary>
    public abstract class SelectHtmlControlBase : Selector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectHtmlControlBase"/> class.
        /// </summary>
        public SelectHtmlControlBase() : base("select")
        {
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RenderEnabledProperty(writer);
            RenderOptionsProperties(writer);
            RenderSelectedValueProperty(writer);

            base.AddAttributesToRender(writer, context);

            RenderChangedEvent(writer);
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
            writer.AddKnockoutDataBind("value", this, SelectedValueProperty, renderEvenInServerRenderingMode: true);
            writer.AddKnockoutDataBind("valueAllowUnset", "true");
        }
    }
}
