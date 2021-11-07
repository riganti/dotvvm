using System;
using System.Collections.Immutable;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Routing
{
    public record DotvvmRouteException(string msg, RouteBase Route, Exception? InnerException = null)
        : DotvvmExceptionBase(msg, InnerException: InnerException);

    public record RouteMissingParametersException(RouteBase Route, ImmutableArray<string> MissingParameters)
        : DotvvmRouteException("Missing route parameters", Route)
    {
        public override string Message => $"The following parameters are not present in route {Route.RouteName}: {string.Join(", ", MissingParameters)}";
    }
}
