#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Container which can host a single page application.
    /// </summary>
    public class SpaContentPlaceHolder : ContentPlaceHolder
    {
        /// <summary>
        /// Gets or sets the default name of the route that should be loaded when there is no hash part in the URL.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [Obsolete("The DefaultRouteName property is not supported - the classic SPA mode (URLs with #/) was removed from DotVVM, and the History API is the default and only option now. See https://www.dotvvm.com/docs/3.0/pages/concepts/layout/single-page-applications-spa#changes-to-spas-in-dotvvm-30 for more details.")]
        public string? DefaultRouteName
        {
            get { return (string?)GetValue(DefaultRouteNameProperty); }
            set { SetValue(DefaultRouteNameProperty, value); }
        }
#pragma warning disable 618
        public static readonly DotvvmProperty DefaultRouteNameProperty
            = DotvvmProperty.Register<string?, SpaContentPlaceHolder>(p => p.DefaultRouteName);
#pragma warning restore 618

        /// <summary>
        /// Gets or sets the name of the route defining the base URL of the SPA (the part of the URL before the hash).
        /// If this property is not set, the URL of the first page using the SpaContentPlaceHolder will stay before the hash mark.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [Obsolete("The PrefixRouteName property is not supported - the classic SPA mode (URLs with #/) was removed from DotVVM, and the History API is the default and only option now. See https://www.dotvvm.com/docs/3.0/pages/concepts/layout/single-page-applications-spa#changes-to-spas-in-dotvvm-30 for more details.")]
        public string? PrefixRouteName
        {
            get { return (string?)GetValue(PrefixRouteNameProperty); }
            set { SetValue(PrefixRouteNameProperty, value); }
        }
#pragma warning disable 618
        public static readonly DotvvmProperty PrefixRouteNameProperty
            = DotvvmProperty.Register<string?, SpaContentPlaceHolder>(c => c.PrefixRouteName, null);
#pragma warning restore 618

        /// <summary>
        /// Gets or sets whether navigation in the SPA pages should use History API.
        /// If this property is not set, settings from <see cref="DotvvmConfiguration">DotvvmConfiguration</see> is used.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [Obsolete("The UseHistoryApi property is not supported - the classic SPA mode (URLs with #/) was removed from DotVVM, and the History API is the default and only option now. See https://www.dotvvm.com/docs/3.0/pages/concepts/layout/single-page-applications-spa#changes-to-spas-in-dotvvm-30 for more details.")]
        public bool? UseHistoryApi
        {
            get { return (bool?)GetValue(UseHistoryApiProperty); }
            set { SetValue(UseHistoryApiProperty, value); }
        }
#pragma warning disable 618
        public static readonly DotvvmProperty UseHistoryApiProperty
            = DotvvmProperty.Register<bool?, SpaContentPlaceHolder>(c => c.UseHistoryApi, null);
#pragma warning restore 618

        public SpaContentPlaceHolder()
        {
            RenderWrapperTag = true;
            WrapperTagName = "div";
        }

        public string GetSpaContentPlaceHolderUniqueId()
        {
            var dotvvmViewId = GetAllAncestors().FirstOrDefault(a => a is DotvvmView).GetType().ToString();
            var markupRelativeFilePath = (string?)GetValue(Internal.MarkupFileNameProperty);

            return HashUtils.HashAndBase64Encode(
                (dotvvmViewId, markupRelativeFilePath, GetDotvvmUniqueId()).ToString());
        }

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            var rootObject = GetRoot();
            rootObject.SetValue(Internal.IsSpaPageProperty, true);

            rootObject.SetValue(Internal.UseHistoryApiSpaNavigationProperty, true);

            Attributes["name"] = HostingConstants.SpaContentPlaceHolderID;
            Attributes[HostingConstants.SpaContentPlaceHolderDataAttributeName] = GetSpaContentPlaceHolderUniqueId();
            
            AddDotvvmUniqueIdAttribute();

            context.ResourceManager.RegisterProcessor(new ResourceManagement.SpaModeResourceProcessor(context.Configuration));

            base.OnInit(context);
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            if (context.IsSpaRequest)
            {
                // we need to render the HTML on postback when SPA request arrives
                SetValue(PostBack.UpdateProperty, true);
            }

            base.OnPreRender(context);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (!context.IsInPartialRenderingMode)
            {
                writer.AddStyleAttribute("display", "none");
            }
            writer.AddKnockoutDataBind("if", "dotvvm.isSpaReady");

            base.AddAttributesToRender(writer, context);
        }

        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidatePlaceholderUsage(ResolvedControl control)
        {
            if (control.Properties.ContainsKey(UseHistoryApiProperty))
            {
                yield return new ControlUsageError("The UseHistoryApi property is not supported - the classic SPA mode (URLs with #/) was removed from DotVVM, and the History API is the default and only option now. See https://www.dotvvm.com/docs/3.0/pages/concepts/layout/single-page-applications-spa#changes-to-spas-in-dotvvm-30 for more details.", control.DothtmlNode);
            }
            if (control.Properties.ContainsKey(DefaultRouteNameProperty))
            {
                yield return new ControlUsageError("The DefaultRouteName property is not supported - the classic SPA mode (URLs with #/) was removed from DotVVM, and the History API is the default and only option now. See https://www.dotvvm.com/docs/3.0/pages/concepts/layout/single-page-applications-spa#changes-to-spas-in-dotvvm-30 for more details.", control.DothtmlNode);
            }
            if (control.Properties.ContainsKey(PrefixRouteNameProperty))
            {
                yield return new ControlUsageError("The PrefixRouteName property is not supported - the classic SPA mode (URLs with #/) was removed from DotVVM, and the History API is the default and only option now. See https://www.dotvvm.com/docs/3.0/pages/concepts/layout/single-page-applications-spa#changes-to-spas-in-dotvvm-30 for more details.", control.DothtmlNode);
            }
        }
    }
}
