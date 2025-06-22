using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the resource links with RenderPosition = Body and the serialized viewmodel. This control must be on every page, usually just before the end of body element.
    /// </summary>
    public class BodyResourceLinks : DotvvmControl
    {
        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // render resource links
            var resourceManager = context.ResourceManager;
            if (resourceManager.BodyRendered) return;
            resourceManager.BodyRendered = true;  // set the flag before the resources are rendered, so they can't add more resources to the list during the render
            ResourcesRenderer.RenderResources(resourceManager, writer, context, ResourceRenderPosition.Body);

            // render the serialized viewmodel
            writer.RenderBeginTag("input");
            writer.WriteAttributeUnbuffered("type"u8, "hidden"u8);
            writer.WriteAttributeUnbuffered("id"u8, "__dot_viewmodel_root"u8);
            var viewModel = context.GetSerializedViewModel();
            writer.WriteAttributeUnbuffered("value"u8, viewModel.Span);
            // using (var vmStream = writer.WriteAttributeUnbuffered("value"u8))
            // {
            //     context.GetSerializedViewModel(vmStream);
            // }
            writer.RenderEndTag();

            // init on load
            byte[] initCode = [ .."window.dotvvm.init("u8, ..StringUtils.Utf8.GetBytes(KnockoutHelper.MakeStringLiteral(CultureInfo.CurrentCulture.Name)), ..");"u8 ];
            var config = context.Configuration;
            if (!config.Runtime.CompressPostbacks.IsEnabledForRoute(context.Route?.RouteName, defaultValue: !config.Debug))
            {
                initCode = [ .."dotvvm.options.compressPOST=false;\n"u8, ..initCode ];
            }
            if (config.ExperimentalFeatures.KnockoutDeferUpdates.IsEnabledForRoute(context.Route?.RouteName))
            {
                initCode = [ .."ko.options.deferUpdates = true;\n"u8, ..initCode ];
            }
            InlineScriptResource.RenderScript(writer, initCode, defer: true, module: true);

            var warnings = RenderWarnings(context);
            if (warnings.Length > 0)
            {
                writer.RenderBeginTag("script");
                writer.WriteUnencodedText(warnings);
                writer.RenderEndTag();
            }
        }

        internal static string RenderWarnings(IDotvvmRequestContext context)
        {
            var result = "";
            // propagate warnings to JS console
            var collector = context.Services.GetService<RuntimeWarningCollector>();
            if (collector is null || !collector.Enabled) return result;

            foreach (var w in collector.GetWarnings())
            {
                var msg = KnockoutHelper.MakeStringLiteral(w.ToString());
                result += $"console.warn({msg});\n";
            }
            return result;
        }
    }
}
