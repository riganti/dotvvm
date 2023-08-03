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

            var initCalls = this.ReferencedModules.Select(m => $"dotvvm.viewModules.init({KnockoutHelper.MakeStringLiteral(m)}, {KnockoutHelper.MakeStringLiteral(viewId)}, document.body);");

            // Run the module init in the init event
            // * dotvvm.state will be available
            // * executed before applying bindings to the controls, so the page module will initialize before control modules 
            this.registrationScript =
                "dotvvm.events.init.subscribeOnce(() => {\n" +
                "    " + string.Join("\n", initCalls) + "\n" +
                "})";
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
