using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Routing;

public interface IPartialMatchRouteHandler
{
    /// <summary>
    /// Handles the partial route match and returns whether the request was handled to prevent other handlers to take place.
    /// </summary>
    /// <returns>If true, the next partial match handlers will not be executed.</returns>
    Task<bool> TryHandlePartialMatch(IDotvvmRequestContext context);
}
