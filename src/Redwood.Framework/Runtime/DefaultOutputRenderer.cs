using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Redwood.Framework.Controls;
using Redwood.Framework.Controls.Infrastructure;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Runtime
{
    public class DefaultOutputRenderer : IOutputRenderer
    {
        public void RenderPage(RedwoodRequestContext context, RedwoodView view)
        {
            // embed resource links
            EmbedResourceLinks(view);

            // prepare the render context
            var renderContext = new RenderContext(context);

            // get the HTML
            using (var textWriter = new StringWriter())
            {
                var htmlWriter = new HtmlWriter(textWriter);
                view.Render(htmlWriter, renderContext);
                context.RenderedHtml = textWriter.ToString();
            }
        }

        public async Task WriteHtmlResponse(RedwoodRequestContext context)
        {
            // return the response
            context.OwinContext.Response.ContentType = "text/html; charset=utf-8";
            await context.OwinContext.Response.WriteAsync(context.RenderedHtml);
        }


        public async Task WriteViewModelResponse(RedwoodRequestContext context, RedwoodView view)
        {
            // return the response
            context.OwinContext.Response.ContentType = "application/json; charset=utf-8";
            var serializedViewModel = context.GetSerializedViewModel();
            await context.OwinContext.Response.WriteAsync(serializedViewModel);
        }



        /// <summary>
        /// Embeds the resource links in the page.
        /// </summary>
        private void EmbedResourceLinks(RedwoodView view)
        {
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