using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Controls
{
    public interface IRenderable
    {
        void Render(IHtmlWriter writer, IDotvvmRequestContext context);
    }
}
