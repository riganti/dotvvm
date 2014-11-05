using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    public class IntegrationScripts : RedwoodControl
    {

        private static readonly string[] scripts =
        {
            "/Scripts/knockout-3.2.0.js",
            "/Scripts/knockout.mapping-latest.js",
            "/Scripts/Redwood.js"
        };


        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            // render default scripts
            foreach (var script in scripts)
            {
                writer.AddAttribute("src", context.RedwoodRequestContext.OwinContext.Request.PathBase + script);
                writer.RenderBeginTag("script");
                writer.RenderEndTag();
            }

            // render the serialized viewmodel
            writer.RenderBeginTag("script");
            writer.WriteUnencodedText("redwood.viewModels.");
            writer.WriteUnencodedText(context.CurrentPageArea);
            writer.WriteUnencodedText("=");
            writer.WriteUnencodedText(context.SerializedViewModel);
            writer.WriteUnencodedText(";");
            writer.WriteUnencodedText(string.Format("redwood.init('{0}');", context.CurrentPageArea));
            writer.RenderEndTag();
        }

        
    }
}
