using System.Collections.Generic;
using System.Security.Claims;
using DotVVM.Framework.Hosting;
using Microsoft.Owin;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpContext : IHttpContext
    {
        public DotvvmHttpContext(IOwinContext originalContext, IAuthentication authentication)
        {
            OriginalContext = originalContext;
            Authentication = authentication;
        }

        public ClaimsPrincipal User { get; set; }
        public IHttpRequest Request { get; set; }
        public IHttpResponse Response { get; set; }
        public IAuthentication Authentication { get; }
        public IDictionary<object, object> Items { get; set; }
        public IOwinContext OriginalContext { get; }
    }
}