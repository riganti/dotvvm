using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a script that is executed when the DotVVM framework is loaded.
    /// </summary>
    public class InlineScript : DotvvmControl
    {
        
        internal override void OnPreRenderComplete(DotvvmRequestContext context)
        {
            EnsureControlHasId();

            if (Children.Count != 1 || !(Children[0] is Literal))
            {
                throw new Exception("The <rw:InlineScript>...</rw:InlineScript> control can only contain text content!");
            }
            
            var script = ((Literal)Children[0]).Text;
            context.ResourceManager.AddStartupScript("inlinescript_" + ID, script, Constants.DotvvmResourceName);
            
            base.OnPreRenderComplete(context);
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            // don't render anything
        }
    }
}
