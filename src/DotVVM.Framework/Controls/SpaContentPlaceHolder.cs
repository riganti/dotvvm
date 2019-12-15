#nullable enable
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;

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
        public string? DefaultRouteName
        {
            get { return (string?)GetValue(DefaultRouteNameProperty); }
            set { SetValue(DefaultRouteNameProperty, value); }
        }
        public static readonly DotvvmProperty DefaultRouteNameProperty
            = DotvvmProperty.Register<string?, SpaContentPlaceHolder>(p => p.DefaultRouteName);

        /// <summary>
        /// Gets or sets the name of the route defining the base URL of the SPA (the part of the URL before the hash).
        /// If this property is not set, the URL of the first page using the SpaContentPlaceHolder will stay before the hash mark.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string? PrefixRouteName
        {
            get { return (string?)GetValue(PrefixRouteNameProperty); }
            set { SetValue(PrefixRouteNameProperty, value); }
        }
        public static readonly DotvvmProperty PrefixRouteNameProperty
            = DotvvmProperty.Register<string?, SpaContentPlaceHolder>(c => c.PrefixRouteName, null);

        /// <summary>
        /// Gets or sets whether navigation in the SPA pages should use History API.
        /// If this property is not set, settings from <see cref="DotvvmConfiguration">DotvvmConfiguration</see> is used.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool? UseHistoryApi
        {
            get { return (bool?)GetValue(UseHistoryApiProperty); }
            set { SetValue(UseHistoryApiProperty, value); }
        }
        public static readonly DotvvmProperty UseHistoryApiProperty
            = DotvvmProperty.Register<bool?, SpaContentPlaceHolder>(c => c.UseHistoryApi, null);

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

            var useHistoryApiSpaNavigation = UseHistoryApi ?? context.Configuration.UseHistoryApiSpaNavigation;
            rootObject.SetValue(Internal.UseHistoryApiSpaNavigationProperty, useHistoryApiSpaNavigation);

            Attributes["name"] = HostingConstants.SpaContentPlaceHolderID;
            Attributes[HostingConstants.SpaContentPlaceHolderDataAttributeName] = GetSpaContentPlaceHolderUniqueId();
            Attributes[HostingConstants.SpaUseHistoryApiAttributeName] = JsonConvert.ToString(useHistoryApiSpaNavigation);

            var correctSpaUrlPrefix = GetCorrectSpaUrlPrefix(context);
            if (correctSpaUrlPrefix != null)
            {
                Attributes[HostingConstants.SpaUrlPrefixAttributeName] = correctSpaUrlPrefix;
            }

            AddDotvvmUniqueIdAttribute();

            base.OnInit(context);
        }

        private string? GetCorrectSpaUrlPrefix(IDotvvmRequestContext context)
        {
            var routePath = "";
            if (!string.IsNullOrEmpty(PrefixRouteName))
            {
                routePath = context.Configuration.RouteTable[PrefixRouteName].Url.Trim('/');
            }
            else
            {
                return null;
            }

            var result = DotvvmMiddlewareBase.GetVirtualDirectory(context.HttpContext);
            if (!string.IsNullOrEmpty(routePath))
            {
                result += "/" + routePath;
            }
            return result;
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

            if (Children.Count > 0)
            {
                writer.AddAttribute(HostingConstants.SpaContentAttributeName, string.Empty);
            }

            if (!string.IsNullOrEmpty(DefaultRouteName))
            {
                var route = context.Configuration.RouteTable[DefaultRouteName];
                if (route.ParameterNames.Any())
                {
                    throw new DotvvmControlException(this, $"The route '{DefaultRouteName}' specified in SpaContentPlaceHolder DefaultRouteName property cannot contain route parameters!");
                }
                writer.AddAttribute(HostingConstants.SpaContentPlaceHolderDefaultRouteDataAttributeName, route.Url);
            }
            base.AddAttributesToRender(writer, context);
        }
    }
}
