using System.IO;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;
using System;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpRequest : IHttpRequest
    {
        public HttpRequest OriginalRequest { get; }

        public IHttpContext HttpContext { get; }

        public DotvvmHttpRequest(HttpRequest originalRequest, IHttpContext httpContext)
        {
            OriginalRequest = originalRequest;
            HttpContext = httpContext;
            Headers = new DotvvmHeaderCollection(originalRequest.Headers);
        }

        public string Method
        {
            get { return OriginalRequest.Method; }
            set { OriginalRequest.Method = value; }
        }

        public string Scheme
        {
            get { return OriginalRequest.Scheme; }
            set { OriginalRequest.Scheme = value; }
        }

        public string ContentType
        {
            get { return OriginalRequest.ContentType; }
            set { OriginalRequest.ContentType = value; }
        }

        public bool IsHttps
        {
            get { return OriginalRequest.IsHttps; }
            set { OriginalRequest.IsHttps = value; }
        }

        public IPathString Path
        {
            get { return new DotvvmHttpPathString(OriginalRequest.Path); }
            set { OriginalRequest.Path = value.HasValue() ? new PathString(value.Value) : PathString.Empty; }
        }
        public IPathString PathBase
        {
            get { return new DotvvmHttpPathString(OriginalRequest.PathBase); }
            set { OriginalRequest.PathBase = value.HasValue() ? new PathString(value.Value) : PathString.Empty; }
        }

        public Stream Body
        {
            get { return OriginalRequest.Body; }
            set { OriginalRequest.Body = value; }
        }

        public string QueryString
        {
            get { return OriginalRequest.QueryString.Value; }
            set { OriginalRequest.QueryString = new QueryString(value); }
        }

        public IQueryCollection Query => new DotvvmQueryCollection(OriginalRequest.Query);
        public ICookieCollection Cookies => new DotvvmCookieCollection(OriginalRequest.Cookies);
        public IHeaderCollection Headers { get; }

        public Uri Url => new Uri(OriginalRequest.GetDisplayUrl());
    }
}