using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Runtime;

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
        public string DefaultRouteName
        {
            get { return (string)GetValue(DefaultRouteNameProperty); }
            set { SetValue(DefaultRouteNameProperty, value); }
        }
        public static readonly DotvvmProperty DefaultRouteNameProperty
            = DotvvmProperty.Register<string, SpaContentPlaceHolder>(p => p.DefaultRouteName);


        /// <summary>
        /// Gets or sets the name of the route defining the base URL of the SPA (the part of the URL before the hash).
        /// If this property is not set, the URL of the first page using the SpaContentPlaceHolder will stay before the hash mark.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string PrefixRouteName
        {
            get { return (string)GetValue(PrefixRouteNameProperty); }
            set { SetValue(PrefixRouteNameProperty, value); }
        }
        public static readonly DotvvmProperty PrefixRouteNameProperty
            = DotvvmProperty.Register<string, SpaContentPlaceHolder>(c => c.PrefixRouteName, null);



        public SpaContentPlaceHolder()
        {
            RenderWrapperTag = true;
            WrapperTagName = "div";
        }

        public string GetSpaContentPlaceHolderUniqueId()
        {
            return GetAllAncestors().FirstOrDefault(a => a is DotvvmView).GetType().ToString();
        }

        protected internal override void OnInit(Hosting.IDotvvmRequestContext context)
        {
            GetRoot().SetValue(Internal.IsSpaPageProperty, true);

            Attributes["name"] = HostingConstants.SpaContentPlaceHolderID;
            Attributes[HostingConstants.SpaContentPlaceHolderDataAttributeName] = GetSpaContentPlaceHolderUniqueId();

            var correctSpaUrlPrefix = GetCorrectSpaUrlPrefix(context);
            if (correctSpaUrlPrefix != null)
            {
                Attributes[HostingConstants.SpaUrlPrefixAttributeName] = correctSpaUrlPrefix;
            }

            AddDotvvmUniqueIdAttribute();

            base.OnInit(context);
        }
        
        private string GetCorrectSpaUrlPrefix(IDotvvmRequestContext context)
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

        protected internal override void OnPreRender(Hosting.IDotvvmRequestContext context)
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