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
        public async Task RenderPage(RedwoodRequestContext context, RedwoodView view, string serializedViewModel)
        {
            // embed resource links
            EmbedResourceLinks(view);

            // prepare the render context
            var renderContext = new RenderContext(context)
            {
                SerializedViewModel = serializedViewModel
            };

            // get the HTML
            using (var textWriter = new StringWriter())
            {
                var htmlWriter = new HtmlWriter(textWriter);
                view.Render(htmlWriter, renderContext);
                var html = textWriter.ToString();

                // return the response
                context.OwinContext.Response.ContentType = "text/html; charset=utf-8";
                await context.OwinContext.Response.WriteAsync(html);
            }
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

            sections[0].Children.Add(new ScriptResourceLinks());
            sections[1].Children.Add(new StylesheetResourceLinks());
        }

        public async Task RenderViewModel(RedwoodRequestContext context, RedwoodView view, string serializedViewModel)
        {
            // return the response
            context.OwinContext.Response.ContentType = "application/json; charset=utf-8";
            await context.OwinContext.Response.WriteAsync(serializedViewModel);
        }
    }
}