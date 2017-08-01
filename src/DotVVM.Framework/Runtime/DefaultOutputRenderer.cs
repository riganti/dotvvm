using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Runtime
{
    public class DefaultOutputRenderer : IOutputRenderer
    {
        protected virtual string RenderPage(IDotvvmRequestContext context, DotvvmView view)
        {
            // embed resource links
            EmbedResourceLinks(view);

            // prepare the render context
            // get the HTML
            using (var textWriter = new StringWriter())
            {
                var htmlWriter = new HtmlWriter(textWriter, context);
                view.Render(htmlWriter, context);
                return textWriter.ToString();
            }
        }

        public virtual async Task WriteHtmlResponse(IDotvvmRequestContext context, DotvvmView view)
        {
            // return the response
            context.HttpContext.Response.ContentType = "text/html; charset=utf-8";
            SetCacheHeaders(context.HttpContext);
            var html = RenderPage(context, view);
            await context.HttpContext.Response.WriteAsync(html);
        }

        public virtual void RenderPostbackUpdatedControls(IDotvvmRequestContext context, DotvvmView page)
        {
            var stack = new Stack<DotvvmControl>();
            stack.Push(page);
            do
            {
                var control = stack.Pop();

                object val;
                if (control.properties != null &&
                    control.properties.TryGetValue(PostBack.UpdateProperty, out val) &&
                    val is bool && (bool)val)
                {
                    using (var w = new StringWriter())
                    {
                        control.Render(new HtmlWriter(w, context), context);

                        var clientId = control.GetDotvvmUniqueId() as string;
                        if (clientId == null)
                        {
                            throw new DotvvmControlException(control, "This control cannot use PostBack.Update=\"true\" because it has dynamic ID. This happens when the control is inside a Repeater or other data-bound control and the RenderSettings.Mode=\"Client\".");
                        }
                        context.PostBackUpdatedControls[clientId] = w.ToString();
                    }
                }
                else
                {
                    foreach (var child in control.Children)
                    {
                        stack.Push(child);
                    }
                }

            } while (stack.Count > 0);
        }


        public virtual async Task WriteViewModelResponse(IDotvvmRequestContext context, DotvvmView view)
        {
            // return the response
            context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
            SetCacheHeaders(context.HttpContext);
            var serializedViewModel = context.GetSerializedViewModel();
            await context.HttpContext.Response.WriteAsync(serializedViewModel);
        }

        public virtual async Task RenderPlainJsonResponse(IHttpContext context, object data)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json; charset=utf-8";
            SetCacheHeaders(context);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(data));
        }

        public virtual async Task RenderHtmlResponse(IHttpContext context, string html)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/html; charset=utf-8";
            SetCacheHeaders(context);
            await context.Response.WriteAsync(html);
        }

        public virtual async Task RenderPlainTextResponse(IHttpContext context, string text)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/plain; charset=utf-8";
            SetCacheHeaders(context);
            await context.Response.WriteAsync(text);
        }

        private static void SetCacheHeaders(IHttpContext context)
        {
            context.Response.Headers["Cache-Control"] = "no-cache";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "-1";
        }


        /// <summary>
        /// Embeds the resource links in the page.
        /// </summary>
        private void EmbedResourceLinks(DotvvmView view)
        {
            // PERF: 
            var sections = view.GetThisAndAllDescendants()
                .OfType<HtmlGenericControl>()
                .Where(t => t.TagName == "head" || t.TagName == "body")
                .OrderBy(t => t.TagName)
                .ToList();

            if (sections.Count != 2 || sections[0].TagName == sections[1].TagName)
            {
                throw new Exception("The page must have exactly one <head> and one <body> section!");
            }

            sections[0].Children.Add(new BodyResourceLinks());
            sections[1].Children.Add(new HeadResourceLinks());
        }

    }
}
