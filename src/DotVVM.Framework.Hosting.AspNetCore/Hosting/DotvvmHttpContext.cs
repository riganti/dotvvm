using System;
using System.Collections.Generic;
using System.Security.Claims;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpContext : IHttpContext
    {
        public DotvvmHttpContext(HttpContext originalContext)
        {
            OriginalContext = originalContext;
        }

        public ClaimsPrincipal User => OriginalContext.User;
        public IHttpRequest Request { get; set; }
        public IHttpResponse Response { get; set; }
        public HttpContext OriginalContext { get; }

        public T GetItem<T>(string key)
        {
            object resultObj;
            if (OriginalContext.Items.TryGetValue(key, out resultObj) && resultObj is T) return (T)resultObj;
            return default(T);
        }

        public void SetItem<T>(string key, T value) => OriginalContext.Items[key] = value;

        public IEnumerable<Tuple<string, IEnumerable<KeyValuePair<string, object>>>> GetEnvironmentTabs()
        {
            yield return Tuple.Create("Features", OriginalContext.Features
                .Select(k => new KeyValuePair<string, object>(k.Key.ToString(), k.Value)));

            yield return Tuple.Create("Items", OriginalContext.Items
                .Select(k => new KeyValuePair<string, object>(k.Key.ToString(), k.Value)));
        }
    }
}