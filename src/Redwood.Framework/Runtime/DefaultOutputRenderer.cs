using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Redwood.Framework.Controls;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Runtime
{
    public class DefaultOutputRenderer : IOutputRenderer
    {
        public async Task RenderPage(RedwoodRequestContext context, RedwoodView view, string serializedViewModel)
        {
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

        public async Task RenderViewModel(RedwoodRequestContext context, RedwoodView view, string serializedViewModel)
        {
            // return the response
            context.OwinContext.Response.ContentType = "application/json; charset=utf-8";
            await context.OwinContext.Response.WriteAsync(serializedViewModel);
        } 
    }
}