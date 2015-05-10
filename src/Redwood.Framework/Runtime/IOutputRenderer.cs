using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using Redwood.Framework.Controls.Infrastructure;
using Redwood.Framework.Hosting;
using Redwood.Framework.Storage;

namespace Redwood.Framework.Runtime
{
    public interface IOutputRenderer
    {

        void RenderPage(RedwoodRequestContext context, RedwoodView view);

        Task WriteHtmlResponse(RedwoodRequestContext context);

        Task WriteViewModelResponse(RedwoodRequestContext context, RedwoodView view);

        Task RenderPlainJsonResponse(IOwinContext context, object data);

        Task RenderHtmlResponse(IOwinContext context, string html);

        Task RenderPlainTextResponse(IOwinContext context, string text);
    }
}