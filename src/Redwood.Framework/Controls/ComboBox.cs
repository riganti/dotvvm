using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;
using Redwood.Framework.Utils;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Renders a HTML drop-down list.
    /// </summary>
    public class ComboBox : Selector
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ComboBox"/> class.
        /// </summary>
        public ComboBox() : base("select")
        {
            
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            if (!RenderOnServer)
            {
                writer.AddKnockoutDataBind("options", this, DataSourceProperty, () => { });

                if (!string.IsNullOrEmpty(DisplayMember))
                {
                    writer.AddKnockoutDataBind("optionsText", KnockoutHelper.MakeStringLiteral(DisplayMember));
                }
                if (!string.IsNullOrEmpty(ValueMember))
                {
                    writer.AddKnockoutDataBind("optionsValue", KnockoutHelper.MakeStringLiteral(ValueMember));
                }
            }

            writer.AddKnockoutDataBind("value", this, SelectedValueProperty, () => { });
            
            base.AddAttributesToRender(writer, context);
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            if (RenderOnServer)
            {
                // render items
                foreach (var item in DataSource)
                {
                    var value = string.IsNullOrEmpty(ValueMember) ? item : ReflectionUtils.GetObjectProperty(item, ValueMember);
                    var text = string.IsNullOrEmpty(DisplayMember) ? item : ReflectionUtils.GetObjectProperty(item, DisplayMember);

                    writer.AddAttribute("value", value != null ? value.ToString() : "");
                    writer.RenderSelfClosingTag("option");
                    writer.WriteText(text != null ? text.ToString() : "");
                    writer.RenderEndTag();
                }
            }
        }
    }
}
