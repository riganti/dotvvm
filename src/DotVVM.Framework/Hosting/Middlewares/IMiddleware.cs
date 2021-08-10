#nullable enable
using System;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public interface IMiddleware
    {
        /// <summary>
        /// Handle given request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Whether the request is handled or if the next middleware should be invoked.</returns>
        Task<bool> Handle(IDotvvmRequestContext request);
    }
}
