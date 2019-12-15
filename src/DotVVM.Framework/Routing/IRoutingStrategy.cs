using System.Collections.Generic;

namespace DotVVM.Framework.Routing
{
    public interface IRoutingStrategy
    {
        IEnumerable<RouteBase> GetRoutes();
    }
}