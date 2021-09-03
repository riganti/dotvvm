using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmCookieCollection : ICookieCollection
    {
        public DotvvmCookieCollection(IRequestCookieCollection originalCookies)
        {
            OriginalCookies = originalCookies;
        }

        public IRequestCookieCollection OriginalCookies { get; }
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return OriginalCookies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return OriginalCookies.GetEnumerator();
        }

        public string this[string key] => OriginalCookies[key];
    }
}