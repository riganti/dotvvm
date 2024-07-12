using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Routing;

public class CanonicalRedirectPartialMatchRouteHandler : IPartialMatchRouteHandler
{
    /// <summary>
    /// Indicates whether a permanent redirect shall be used.
    /// </summary>
    public bool IsPermanentRedirect { get; set; }

    public Task<bool> TryHandlePartialMatch(IDotvvmRequestContext context)
    {
        context.RedirectToRoute(context.Route!.RouteName, context.Parameters);
        return Task.FromResult(true);
    }
}
