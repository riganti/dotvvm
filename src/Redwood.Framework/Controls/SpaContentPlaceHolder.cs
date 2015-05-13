using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls.Infrastructure;
using Redwood.Framework.Hosting;
using Redwood.Framework.Parser;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    public class SpaContentPlaceHolder : ContentPlaceHolder
    {

        public string DefaultRouteName
        {
            get { return (string)GetValue(DefaultRouteNameProperty); }
            set { SetValue(DefaultRouteNameProperty, value); }
        }

        public static readonly RedwoodProperty DefaultRouteNameProperty
            = RedwoodProperty.Register<string, SpaContentPlaceHolder>(p => p.DefaultRouteName);


        public string GetSpaContentPlaceHolderUniqueId()
        {
            return GetAllAncestors().FirstOrDefault(a => a is RedwoodView).GetType().ToString();
        }

        protected internal override void OnPreRender(RedwoodRequestContext context)
        {
            if (context.IsSpaRequest)
            {
                // we need to render the HTML on postback when SPA request arrives
                SetValue(PostBack.UpdateProperty, true);
            }

            base.OnPreRender(context);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddAttribute("id", ID);
            writer.AddAttribute("name", Constants.SpaContentPlaceHolderID);
            writer.AddAttribute(Constants.SpaContentPlaceHolderDataAttributeName, GetSpaContentPlaceHolderUniqueId());

            if (!string.IsNullOrEmpty(DefaultRouteName))
            {
                var route = context.RequestContext.Configuration.RouteTable[DefaultRouteName];
                if (route.ParameterNames.Any())
                {
                    throw new ArgumentException(string.Format("The route {0} specified in SpaContentPlaceHolder DefaultRouteName property cannot contain route parameters!", DefaultRouteName));
                }
                writer.AddAttribute(Constants.SpaContentPlaceHolderDefaultRouteDataAttributeName, route.Url);
            }
            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderBeginTag(IHtmlWriter writer, RenderContext context)
        {
            writer.RenderBeginTag("div");
            base.RenderBeginTag(writer, context);
        }

        protected override void RenderEndTag(IHtmlWriter writer, RenderContext context)
        {
            base.RenderEndTag(writer, context);
            writer.RenderEndTag();
        }
    }
}