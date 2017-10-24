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
            var resultSuffix = hashIndex < 0 ? urlSuffix : urlSuffix.Substring(0, hashIndex);

            switch (query)
            {
                case null:
                    break;
                case IEnumerable<KeyValuePair<string, string>> keyValueCollection:
                    foreach (var item in keyValueCollection)
                    {
                        resultSuffix = resultSuffix + (resultSuffix.LastIndexOf('?') < 0 ? "?" : "&") +
                                       Uri.EscapeDataString(item.Key) +
                                       "=" + Uri.EscapeDataString(item.Value);
                    }
                    break;
                case IEnumerable<KeyValuePair<string, object>> keyValueCollection:
                    foreach (var item in keyValueCollection)
                    {
                        resultSuffix = resultSuffix + (resultSuffix.LastIndexOf('?') < 0 ? "?" : "&") +
                                       Uri.EscapeDataString(item.Key) +
                                       "=" + Uri.EscapeDataString(item.Value.ToString());
                    }
                    break;
                default:
                    foreach (var prop in query.GetType().GetProperties())
                    {
                        resultSuffix = resultSuffix + (resultSuffix.LastIndexOf('?') < 0 ? "?" : "&") +
                                       Uri.EscapeDataString(prop.Name) +
                                       "=" + Uri.EscapeDataString(prop.GetValue(query).ToString());
                    }
                    break;
            }

            return resultSuffix + (hashIndex < 0 ? "" : urlSuffix.Substring(hashIndex));
        }
    }
}
