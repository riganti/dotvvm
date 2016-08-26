using System.IO;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpRequest : IHttpRequest
    {
        public HttpRequest OriginalRequest { get; }

        public IHttpContext HttpContext { get; }


        public DotvvmHttpRequest(HttpRequest originalRequest, IHttpContext httpContext,
            IPathString path, IPathString pathBase, IQueryCollection query, IHeaderCollection headers )
        {
            OriginalRequest = originalRequest;
            HttpContext = httpContext;
            PathBase = pathBase;
            Path = path;
            Query = query;
            Headers = headers;
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

        public string GetDisplayUrl()
        {
            return OriginalRequest.GetDisplayUrl();
        }

        public IPathString Path { get; set; }
        public IPathString PathBase { get; set; }

        public Stream Body
        {
            get { return OriginalRequest.Body; }
            set { OriginalRequest.Body = value; }
        }

        public IQueryCollection Query { get; }
        public IHeaderCollection Headers { get; }
    }
}