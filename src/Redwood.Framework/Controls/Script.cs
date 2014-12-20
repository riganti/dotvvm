using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    class Script : RedwoodControl
    {
        public string Name { get; set; }

        public override void PrepareRender(RenderContext renderContext)
        {
            renderContext.ResourceManager.AddResource(Name);
            base.PrepareRender(renderContext);
        }
    }
}
