using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Renders the script elements and the serialized viewmodel. This control must be on every page just before the end of body element.
    /// </summary>
    public class ResourceLinks : RedwoodControl
    {

        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            // render resource links
            context.ResourceManager.Render(writer);

            // render the serialized viewmodel
            writer.RenderBeginTag("script");
            writer.WriteUnencodedText("redwood.viewModels.");
            writer.WriteUnencodedText(context.CurrentPageArea);
            writer.WriteUnencodedText("=");
            writer.WriteUnencodedText(context.SerializedViewModel);
            writer.WriteUnencodedText(";\r\n");

            // init on load
            writer.WriteUnencodedText(string.Format("$(function () {{ redwood.init('{0}'); }})", context.CurrentPageArea));
            writer.RenderEndTag();
        }
    }
}
