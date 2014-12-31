using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Hosting;

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

        internal override void OnPreRenderComplete(RedwoodRequestContext context)
        {
            context.ResourceManager.AddResource(Name);
            base.OnPreRenderComplete(context);
        }
    }
}
