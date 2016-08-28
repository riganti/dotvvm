using System.Collections.Generic;
using System.Security.Claims;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpContext : IHttpContext
    {
        public DotvvmHttpContext(HttpContext originalContext, IAuthentication authentication)
        {
            OriginalContext = originalContext;
            Authentication = authentication;
        }

        public ClaimsPrincipal User { get; set; }
        public IHttpRequest Request { get; set; }
        public IHttpResponse Response { get; set; }
        public IAuthentication Authentication { get; }
        public IDictionary<object, object> Items { get; set; }
        public HttpContext OriginalContext { get; }
    }
}