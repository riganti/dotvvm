using System.Collections.Generic;

namespace DotVVM.Framework.Diagnostics.Models
{
    public class HttpHeaderItem
    {
        public HttpHeaderItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }

        public static HttpHeaderItem FromKeyValuePair(KeyValuePair<string, string[]> pair)
        {
            return new HttpHeaderItem(pair.Key, string.Join("; ", pair.Value));
        }
    }
}
