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
        /// <param name="next"></param>
        /// <returns></returns>
        Task Handle(IDotvvmRequestContext request, Func<IDotvvmRequestContext, Task> next);
    }
}