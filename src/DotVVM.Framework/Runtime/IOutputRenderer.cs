using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Storage;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Runtime
{
    public interface IOutputRenderer
    {

        Task WriteHtmlResponse(DotvvmRequestContext context, DotvvmView view);

        Task WriteViewModelResponse(DotvvmRequestContext context, DotvvmView view);

        Task RenderPlainJsonResponse(HttpContext context, object data);

        Task RenderHtmlResponse(HttpContext context, string html);

        Task RenderPlainTextResponse(HttpContext context, string text);
        void RenderPostbackUpdatedControls(DotvvmRequestContext context, DotvvmView page);
    }
}