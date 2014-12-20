using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Hosting
{
    public class DefaultOutputRenderer : IOutputRenderer
    {
        public async Task RenderPage(RedwoodRequestContext context, RedwoodView view, string serializedViewModel)
        {
            var renderContext = new RenderContext(context)
            {
                SerializedViewModel = serializedViewModel
            };
            // prepare view to rendering
            view.PrepareRender(renderContext);

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

        public async Task RenderViewModel(RedwoodRequestContext context, RedwoodView view, string serializedViewModel)
        {
            // return the response
            context.OwinContext.Response.ContentType = "application/json; charset=utf-8";
            await context.OwinContext.Response.WriteAsync(serializedViewModel);
        } 
    }
}