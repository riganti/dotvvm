using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Storage;

namespace DotVVM.Framework.Runtime
{
    public interface IOutputRenderer
    {

        Task WriteHtmlResponse(IDotvvmRequestContext context, DotvvmView view);

        Task WriteViewModelResponse(IDotvvmRequestContext context, DotvvmView view);

        Task RenderPlainJsonResponse(IHttpContext context, object data);

        Task RenderHtmlResponse(IHttpContext context, string html);

        Task RenderPlainTextResponse(IHttpContext context, string text);
        void RenderPostbackUpdatedControls(IDotvvmRequestContext context, DotvvmView page);
    }
}