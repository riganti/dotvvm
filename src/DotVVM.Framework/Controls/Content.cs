#nullable enable
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

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.RenderBeginTag(writer, context);

            var viewModule = this.GetValue<ViewModuleReferenceInfo>(Internal.ReferencedViewModuleInfoProperty);
            if (viewModule is object)
            {
                var settings = DefaultSerializerSettingsProvider.Instance.GetSettingsCopy();
                settings.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

                writer.WriteKnockoutDataBindComment("dotvvm-with-view-modules",
                    $"{{ viewId: {KnockoutHelper.MakeStringLiteral(viewModule.ViewId)}, modules: {JsonConvert.SerializeObject(viewModule.ReferencedModules, settings)} }}"
                );
            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var viewModule = this.GetValue<ViewModuleReferenceInfo>(Internal.ReferencedViewModuleInfoProperty);
            if (viewModule is object)
            {
                writer.WriteKnockoutDataBindEndComment();
            }

            base.RenderEndTag(writer, context);
        }

    }
}
