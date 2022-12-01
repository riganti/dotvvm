using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Contains markup that will be placed inside the according ContentPlaceHolder in the master page.
    /// </summary>
    public class Content : DotvvmControl
    {
        /// <summary>
        /// Gets or sets the ID of the ContentPlaceHolder control in the master page in which the content will be placed.
        /// </summary>
        public string? ContentPlaceHolderID
        {
            get { return (string?)GetValue(ContentPlaceHolderIDProperty); }
            set { SetValue(ContentPlaceHolderIDProperty, value); }
        }
        public static readonly DotvvmProperty ContentPlaceHolderIDProperty =
            DotvvmProperty.Register<string?, Content>(c => c.ContentPlaceHolderID);


        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            base.OnPreRender(context);

            var viewModule = this.GetValue<ViewModuleReferenceInfo>(Internal.ReferencedViewModuleInfoProperty);
            if (viewModule is object)
            {
                Debug.Assert(!viewModule.IsMarkupControl);
                context.ResourceManager.AddRequiredResource(viewModule.ImportResourceName);
            }
        }

        bool hasViewModule = false;

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.RenderBeginTag(writer, context);

            var viewModule = this.GetValue<ViewModuleReferenceInfo>(Internal.ReferencedViewModuleInfoProperty);
            // some users put ContentPlaceHolder into the <head> element, but knockout comments maybe cause problems in there
            // we rather check if we are in head and ignore the @js directive there 
            var isInHead = this.GetAllAncestors().OfType<HtmlGenericControl>().Any(c => "head".Equals(c.TagName,StringComparison.OrdinalIgnoreCase));
            if (viewModule is object && !isInHead)
            {
                hasViewModule = true;
                var settings = DefaultSerializerSettingsProvider.Instance.GetSettingsCopy();
                settings.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

                writer.WriteKnockoutDataBindComment("dotvvm-with-view-modules",
                    $"{{ viewId: {KnockoutHelper.MakeStringLiteral(viewModule.ViewId)}, modules: {JsonConvert.SerializeObject(viewModule.ReferencedModules, settings)} }}"
                );
            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (hasViewModule)
            {
                writer.WriteKnockoutDataBindEndComment();
            }

            base.RenderEndTag(writer, context);
        }

    }
}
