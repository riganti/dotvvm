using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Reference to a list of modules that will be initialized using dotvvm.initModule for runtime use.
    /// </summary>
    public class ViewModuleInitResource : IResource
    {

        public ResourceRenderPosition RenderPosition => ResourceRenderPosition.Anywhere;

        public string[] ReferencedModules { get; }

        public string[] Dependencies { get; }

        public string ResourceName { get; }

        private string registrationScript;

        public ViewModuleInitResource(string[] referencedModules, string name, string viewId, string[] dependencies)
        {
            this.ResourceName = name;
            this.ReferencedModules = referencedModules.ToArray();
            this.Dependencies = dependencies;

            this.registrationScript = @$"dotvvm.events.initCompleted.subscribe(function () {{
    {string.Join("\r\n", this.ReferencedModules.Select(m => $"dotvvm.viewModules.init({KnockoutHelper.MakeStringLiteral(m)}, {KnockoutHelper.MakeStringLiteral(viewId)}, document.body);"))}
}});";
        }

        public void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {

        writer.AddAttribute("type", "module");
            writer.RenderBeginTag("script");
            writer.WriteUnencodedText(registrationScript);
            writer.RenderEndTag();
        }

        public static string GetName(string moduleBatchUniqueId)
        {
            return "viewModule.init." + moduleBatchUniqueId;
        }
    }
}
