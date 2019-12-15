using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Testing
{
    internal class TestHttpContext : IHttpContext
    {
        public TestHttpContext()
        {
            this.Request = new TestHttpRequest(this);
            this.Response = new TestHttpResponse(this);
        }
        public ClaimsPrincipal User { get; set; }

        public TestHttpRequest Request { get; set; }
        IHttpRequest IHttpContext.Request => Request;

        public TestHttpResponse Response { get; set; }
        IHttpResponse IHttpContext.Response => Response;

        public IEnumerable<Tuple<string, IEnumerable<KeyValuePair<string, object>>>> GetEnvironmentTabs() => EnvironmentTabs;

        public List<Tuple<string, IEnumerable<KeyValuePair<string, object>>>> EnvironmentTabs { get; set; } = new List<Tuple<string, IEnumerable<KeyValuePair<string, object>>>>();


        private Dictionary<string, object> items = new Dictionary<string, object>();

        public T GetItem<T>(string key)
        {
            return (T)items[key];
        }

        public void SetItem<T>(string key, T value)
        {
            items[key] = value;
        }
    }
}
