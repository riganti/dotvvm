using System.Collections.Generic;

namespace DotVVM.Framework.Diagnostics.Models
{
    public class HttpHeaderItem
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public static HttpHeaderItem FromKeyValuePair(KeyValuePair<string, string[]> pair)
        {
            return new HttpHeaderItem
            {
                Key = pair.Key,
                Value = string.Join("; ", pair.Value)
            };
        }
    }
}