using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls.Infrastructure;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Declares a resource that will be rendered in the <see cref="BodyResourceLinks" /> control later.
    /// </summary>
    public class RequiredResource : RedwoodControl
    {

        /// <summary>
        /// Gets or sets the name of the resource.
        /// </summary>
        public string Name { get; set; }



        /// <summary>
        /// Called right before the rendering shall occur.
        /// </summary>
        internal override void OnPreRenderComplete(RedwoodRequestContext context)
        {
            context.ResourceManager.AddRequiredResource(Name);
            base.OnPreRenderComplete(context);
        }
    }
}
