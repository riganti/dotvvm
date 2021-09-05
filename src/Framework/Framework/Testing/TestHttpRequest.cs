using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Testing
{
    internal class TestHttpRequest : IHttpRequest
    {
        public TestHttpRequest(IHttpContext context)
        {
            this.HttpContext = context;
        }

        public IHttpContext HttpContext { get; }

        public string Method { get; set; } = "GET";

        public string Scheme { get; set; } = "http";

        public string? ContentType { get; set; }

        public bool IsHttps { get; set; }

        public string? Path { get; set; }

        public string? PathBase { get; set; }
        IPathString IHttpRequest.Path => new TestPathString(this.Path);

        IPathString IHttpRequest.PathBase => new TestPathString(this.PathBase);

        public Stream Body { get; set; } = new MemoryStream();

        public TestQueryCollection Query { get; set; } = new TestQueryCollection();
        IQueryCollection IHttpRequest.Query => this.Query;

        public string? QueryString { get; set; }

        public TestCookieCollection Cookies { get; } = new TestCookieCollection();

        public TestHeaderCollection Headers { get; } = new TestHeaderCollection();
        ICookieCollection IHttpRequest.Cookies => Cookies;
        IHeaderCollection IHttpRequest.Headers => Headers;

        public Uri Url => new UriBuilder(Scheme, "localhost", 80, Path).Uri;

    }
}
