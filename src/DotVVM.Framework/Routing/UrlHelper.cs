using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DotVVM.Framework.Routing
{
    public static class UrlHelper
    {
        /// <summary>
        /// Returns url suffix from a string value and query string parameters
        /// </summary>
        public static string BuildUrlSuffix(string urlSuffix, object query)
        {
            urlSuffix = urlSuffix ?? "";

            var hashIndex = urlSuffix.IndexOf('#');
            var suffixContainsHash = hashIndex >= 0;
            var resultSuffix = !suffixContainsHash ? urlSuffix : urlSuffix.Substring(0, hashIndex);

            switch (query)
            {
                case null:
                    break;
                case IEnumerable<KeyValuePair<string, string>> keyValueCollection:
                    foreach (var item in keyValueCollection)
                    {
                        AppendQueryParam(ref resultSuffix, item.Key, item.Value);
                    }
                    break;
                case IEnumerable<KeyValuePair<string, object>> keyValueCollection:
                    foreach (var item in keyValueCollection)
                    {
                        AppendQueryParam(ref resultSuffix, item.Key, item.Value.ToString());
                    }
                    break;
                default:
                    foreach (var prop in query.GetType().GetProperties())
                    {
                        AppendQueryParam(ref resultSuffix, prop.Name, prop.GetValue(query).ToString());
                    }
                    break;
            }

            return resultSuffix + (!suffixContainsHash ? "" : urlSuffix.Substring(hashIndex));
        }

        private static string AppendQueryParam(ref string urlSuffix, string name, string value)
            => urlSuffix += (urlSuffix.LastIndexOf('?') < 0 ? "?" : "&") + $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";
    }
}
