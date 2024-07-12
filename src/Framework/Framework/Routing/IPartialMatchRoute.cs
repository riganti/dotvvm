using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DotVVM.Framework.Routing;

public interface IPartialMatchRoute
{
    bool IsPartialMatch(string url, [MaybeNullWhen(false)] out RouteBase matchedRoute, [MaybeNullWhen(false)] out IDictionary<string, object?> values);
}
