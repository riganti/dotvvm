using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
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

        public ViewModuleReferencedModule[] ReferencedModules { get; }

        public string[] Dependencies { get; }

        public string ResourceName { get; }

        private readonly string registrationScript;

        public ViewModuleInitResource(ViewModuleReferencedModule[] referencedModules, string name, string viewId, string[] dependencies)
        {
            this.ResourceName = name;
            this.ReferencedModules = referencedModules.ToArray();
            this.Dependencies = dependencies;

            this.registrationScript = string.Join("\r\n", this.ReferencedModules.Select(m =>
            {
                var args = new List<string>()
                {
                    KnockoutHelper.MakeStringLiteral(m.ModuleName),
                    KnockoutHelper.MakeStringLiteral(viewId),
                    "document.body"
                };
                if (m.InitArguments != null)
                {
                    args.Add($"[{string.Join(", ", m.InitArguments.Select(a => KnockoutHelper.MakeStringLiteral(a)))}]");
                }
                return $"dotvvm.viewModules.init({string.Join(", ", args)});";
            }));
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
