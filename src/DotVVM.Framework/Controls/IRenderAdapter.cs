#nullable enable
using System;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    public interface IRenderAdapter
    {
        /// <summary>
        /// Alternative implementation for AddAttributesToRender
        /// </summary>
        Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext>? AddAttributesToRender { get; }

        /// <summary>
        /// Alternative implementation for RenderBeginTag
        /// </summary>
        Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext>? RenderBeginTag { get; }

        /// <summary>
        /// Alternative implementation for RenderContents
        /// </summary>
        Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext>? RenderContents { get; }

        /// <summary>
        /// Alternative implementation for RenderEndTag
        /// </summary>
        Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext>? RenderEndTag { get; }
    }
}
