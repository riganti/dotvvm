using System;
using System.Buffers;
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

        Task WriteViewModelResponse(IDotvvmRequestContext context, DotvvmView view, ReadOnlyMemory<byte> viewModelJson);

        Task WriteStaticCommandResponse(IDotvvmRequestContext context, ReadOnlyMemory<byte> json);

        Task RenderPlainJsonResponse(IHttpContext context, ReadOnlyMemory<byte> json);

        Task RenderHtmlResponse(IHttpContext context, string html);

        Task RenderPlainTextResponse(IHttpContext context, string text);

        IEnumerable<(string name, Action<ReadOnlySpanAction<byte, string>> html)> RenderPostbackUpdatedControls(IDotvvmRequestContext context, DotvvmView page);
    }
}
