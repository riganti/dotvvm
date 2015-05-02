using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Hosting;
using Redwood.Framework.Parser;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Renders a script that is executed when the Redwood framework is loaded.
    /// </summary>
    public class InlineScript : RedwoodControl
    {
        
        internal override void OnPreRenderComplete(RedwoodRequestContext context)
        {
            if (Children.Count != 1 || !(Children[0] is Literal))
            {
                throw new Exception("The <rw:InlineScript>...</rw:InlineScript> control can only contain text content!");
            }
            
            var script = ((Literal)Children[0]).Text;
            context.ResourceManager.AddStartupScript(Guid.NewGuid().ToString(), script, Constants.RedwoodResourceName);
            
            base.OnPreRenderComplete(context);
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            // don't render anything
        }
    }
}
