using System;
using System.IO;

namespace DotVVM.Framework.Hosting
{
    public interface IHttpRequest
    {
        IHttpContext HttpContext { get; }
        string Method { get; }
        [Obsolete("Use the Url.Scheme property instead")]
        string Scheme { get; }
        string? ContentType { get; }
        bool IsHttps { get; }
        IPathString Path { get; }
        IPathString PathBase { get; }
        Stream Body { get; }
        IQueryCollection Query { get; }
        [Obsolete("Use the Url.Query property instead")]
        string? QueryString { get; }
        ICookieCollection Cookies { get; }
        IHeaderCollection Headers { get; }
		Uri Url { get; }
    }
}
