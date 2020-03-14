#nullable enable
using System;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    public interface IRenderAdapter
    {
        Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext>? AddAttributesToRender { get; }
        Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext>? RenderBeginTag { get; }
        Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext>? RenderContents { get; }
        Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext>? RenderEndTag { get; }
    }
}
