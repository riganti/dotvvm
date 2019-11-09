#nullable enable
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Represents a resources that can claimed from html page.
    /// </summary>
    public interface IResource
    {
        void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName);
        ResourceRenderPosition RenderPosition { get; }
        string[] Dependencies { get; }
    }
}
