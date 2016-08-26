using System.Security.Claims;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpContext : IHttpContext
    {
        public DotvvmHttpContext(HttpContext originalContext, IHttpRequest request, IHttpResponse response, IAuthentication authentication)
        {
            OriginalContext = originalContext;
            Request = request;
            Response = response;
            Authentication = authentication;
        }

        public ClaimsPrincipal User { get; set; }
        public IHttpRequest Request { get; set; }
        public IHttpResponse Response { get; set; }
        public IAuthentication Authentication { get; }
        public HttpContext OriginalContext { get; }
    }
}