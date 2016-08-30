using System.Collections.Generic;
using System.Security.Claims;

namespace DotVVM.Framework.Hosting
{
    public interface IHttpContext
    {
        ClaimsPrincipal User { get; set; }
        IHttpRequest Request { get; }
        IHttpResponse Response { get; }
        IAuthentication Authentication { get; }
        IDictionary<object, object> Items { get; }
    }
}