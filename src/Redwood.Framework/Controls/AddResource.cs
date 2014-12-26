using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Adds resource to resource manager
    /// it will be rendered in rw:ResourceLinks
    /// </summary>
    public class AddResource : RedwoodControl
    {
        /// <summary>
        /// name of the resource
        /// </summary>
        public string Name { get; set; }

        public override void PrepareRender(RenderContext renderContext)
        {
            renderContext.ResourceManager.AddResource(Name);
            base.PrepareRender(renderContext);
        }
    }
}
