using System;
using System.IO;

namespace DotVVM.Framework.Hosting
{
    public interface IHttpRequest
    {
        IHttpContext HttpContext { get; }
        string Method { get; set; }
        string Scheme { get; set; }
        string ContentType { get; set; }
        bool IsHttps { get; set; }
        IPathString Path { get; set; }
        IPathString PathBase { get; set; }
        Stream Body { get; set; }
        IQueryCollection Query { get; }
        ICookieCollection Cookies { get; set; }
        IHeaderCollection Headers { get; }
		Uri Url { get; }
    }
}