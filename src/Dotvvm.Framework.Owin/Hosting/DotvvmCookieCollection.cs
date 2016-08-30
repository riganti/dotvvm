using Microsoft.Owin;
using System.Collections;
using System.Collections.Generic;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmCookieCollection : ICookieCollection
    {
        public DotvvmCookieCollection(RequestCookieCollection originalCookies)
        {
            OriginalCookies = originalCookies;
        }

        public RequestCookieCollection OriginalCookies { get; }
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