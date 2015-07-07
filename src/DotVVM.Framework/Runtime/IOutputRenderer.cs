using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Storage;

namespace DotVVM.Framework.Runtime
{
    public interface IOutputRenderer
    {

        void RenderPage(DotvvmRequestContext context, DotvvmView view);

        Task WriteHtmlResponse(DotvvmRequestContext context);

        Task WriteViewModelResponse(DotvvmRequestContext context, DotvvmView view);

        Task RenderPlainJsonResponse(IOwinContext context, object data);

        Task RenderHtmlResponse(IOwinContext context, string html);

        Task RenderPlainTextResponse(IOwinContext context, string text);
    }
}