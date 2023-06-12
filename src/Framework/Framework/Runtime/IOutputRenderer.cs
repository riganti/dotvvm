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

        Task WritePlainJsonResponse(IHttpContext context, string json);

        Task WritePlainTextResponse(IHttpContext context, string text);

        IEnumerable<(string name, string html)> RenderPostbackUpdatedControls(IDotvvmRequestContext context, DotvvmView page);
    }
}
