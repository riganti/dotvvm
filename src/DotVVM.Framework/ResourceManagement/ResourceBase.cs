#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DotVVM.Framework.Compilation.Parser;
using Newtonsoft.Json;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    public abstract class ResourceBase : IResource
    {
        /// <summary>
        /// Gets or sets the collection of dependent resources.
        /// </summary>
        public string[] Dependencies { get; set; } = new string[0];

        /// <summary>
        /// Gets or sets where the resource has to be 
        /// </summary>
        public ResourceRenderPosition RenderPosition { get; set; }

        public ResourceBase(ResourceRenderPosition renderPosition)
        {
            this.RenderPosition = renderPosition;
        }

        /// <summary>
        /// Renders the resource in the specified <see cref="IHtmlWriter"/>.
        /// </summary>
        public abstract void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName);
    }
}
