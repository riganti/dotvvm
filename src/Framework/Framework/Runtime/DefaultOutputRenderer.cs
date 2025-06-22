using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Binding;
using System.Text;
using DotVVM.Framework.Configuration;
using System.Buffers;
using DotVVM.Framework.Utils;
using System.Diagnostics;

namespace DotVVM.Framework.Runtime
{
    public class DefaultOutputRenderer : IOutputRenderer
    {
        protected virtual MemoryStream RenderPage(IDotvvmRequestContext context, DotvvmView view)
        {
            var outStream = new MemoryStream();
            using (var htmlWriter = new HtmlWriter(outStream, context, leaveStreamOpen: true))
            {
                view.Render(htmlWriter, context);
            }
            outStream.Position = 0;
            return outStream;
        }

        public virtual async Task WriteHtmlResponse(IDotvvmRequestContext context, DotvvmView view)
        {
            // return the response
            context.HttpContext.Response.ContentType = "text/html; charset=utf-8";
            SetCacheHeaders(context.HttpContext);
            using (var html = RenderPage(context, view))
            {
                CheckRenderedResources(context);
                context.HttpContext.Response.Headers["Content-Length"] = html.Length.ToString();
                await html.CopyToAsync(context.HttpContext.Response.Body);
            }
        }

        private void CheckRenderedResources(IDotvvmRequestContext context)
        {
            var resourceManager = context.ResourceManager;
            if (!resourceManager.BodyRendered || !resourceManager.HeadRendered)
                throw new Exception($"Required resources were not rendered, make sure that page contains <head> and <body> elements or <dot:HeadResourceLinks> and <dot:BodyResourceLinks> controls.");
        }

        public virtual IEnumerable<(string name, Action<ReadOnlySpanAction<byte, string>> html)> RenderPostbackUpdatedControls(IDotvvmRequestContext context, DotvvmView page)
        {
            var stack = new Stack<DotvvmControl>();
            stack.Push(page);
            Utf8StringWriter? utf8 = null;
            HtmlWriter? htmlWriter = null;
            try
            {
                do
                {
                    var control = stack.Pop();

                    if (control.properties.TryGet(PostBack.UpdateProperty, out var val) && true.Equals(val))
                    {
                        var clientId = control.GetDotvvmUniqueId().ValueOrDefault;
                        if (clientId == null)
                        {
                            throw new DotvvmControlException(control, "This control cannot use PostBack.Update=\"true\" because it has dynamic ID. This happens when the control is inside a Repeater or other data-bound control and the RenderSettings.Mode=\"Client\".");
                        }
                        Action<ReadOnlySpanAction<byte, string>> renderHtml = (spanCallback) => {
                            if (htmlWriter is null)
                            {
                                utf8 = new Utf8StringWriter();
                                htmlWriter = new HtmlWriter(utf8, context);
                            }
                            else
                            {
                                htmlWriter.Reset();
                                Debug.Assert(utf8!.PendingBytes.IsEmpty);
                            }

                            control.Render(htmlWriter, context);

                            htmlWriter.Reset();
                            var result = utf8.PendingBytes;

                            if (StringUtils.IsEmptyOrAsciiWhiteSpace(result))
                            {
                                throw new DotvvmControlException(control, "The PostBack.Update=\"true\" property is set on this control, but the control does not render anything. ");
                            }
                            if (MemoryExtensions.IndexOf(result, clientId.ToUtf8Bytes()) < 0)
                            {
                                throw new DotvvmControlException(control, "The PostBack.Update=\"true\" property is set on this control, but the control does not render the correct client ID.");
                            }

                            spanCallback(result, clientId);
                        };

                        yield return (clientId, renderHtml);
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
            finally
            {
                htmlWriter?.Dispose();
                utf8?.Dispose();
            }
        }


        public virtual async Task WriteViewModelResponse(IDotvvmRequestContext context, DotvvmView view, ReadOnlyMemory<byte> serializedViewModel)
        {
            // return the response
            context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
            SetCacheHeaders(context.HttpContext);
            await context.HttpContext.Response.WriteAsync(serializedViewModel);
        }

        public virtual async Task WriteStaticCommandResponse(IDotvvmRequestContext context, ReadOnlyMemory<byte> json)
        {
            context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
            SetCacheHeaders(context.HttpContext);
            await context.HttpContext.Response.WriteAsync(json);
        }

        public virtual async Task RenderPlainJsonResponse(IHttpContext context, ReadOnlyMemory<byte> json)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json; charset=utf-8";
            SetCacheHeaders(context);
            await context.Response.WriteAsync(json);
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

        protected virtual void SetCacheHeaders(IHttpContext context)
        {
            context.Response.Headers["Cache-Control"] = "no-cache, no-store";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "-1";
        }
    }
}
