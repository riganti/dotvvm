using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls.Infrastructure
{
    public class GlobalizeResource : DotvvmControl
    {
        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            context.ResourceManager.AddCurrentCultureGlobalizationResource();
            base.OnPreRender(context);
        }
    }
}
