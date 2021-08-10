#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Hosting
{
    public interface IDotvvmPresenter
    {
        /// <summary>
        /// Processes the request.
        /// </summary>
        Task ProcessRequest(IDotvvmRequestContext context);
    }
}
