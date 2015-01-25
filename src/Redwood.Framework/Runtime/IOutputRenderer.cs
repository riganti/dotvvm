using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redwood.Framework.Controls.Infrastructure;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Runtime
{
    public interface IOutputRenderer
    {

        void RenderPage(RedwoodRequestContext context, RedwoodView view);

        Task WriteHtmlResponse(RedwoodRequestContext context);

        Task WriteViewModelResponse(RedwoodRequestContext context, RedwoodView view);

    }
}