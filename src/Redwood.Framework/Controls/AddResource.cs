using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    public class AddResource : RedwoodControl
    {
        public string Name { get; set; }

        public override void PrepareRender(RenderContext renderContext)
        {
            renderContext.ResourceManager.AddResource(Name);
            base.PrepareRender(renderContext);
        }
    }
}
