using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Routing
{
    public static class UrlHelper
    {
        /// <summary>
        /// Returns url suffix from a string value and query string parameters
        /// </summary>
        public static string BuildUrlSuffix(string? urlSuffix, object? query)
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
                    foreach (var item in keyValueCollection.Where(i => i.Value != null))
                    {
                        AppendQueryParam(ref resultSuffix, item.Key, item.Value.ToString().NotNull());
                    }
                    break;
                default:
                    foreach (var prop in query.GetType().GetProperties())
                    {
                        AppendQueryParam(ref resultSuffix, prop.Name, prop.GetValue(query)!.ToString().NotNull());
                    }
                    break;
            }

            return resultSuffix + (!suffixContainsHash ? "" : urlSuffix.Substring(hashIndex));
        }

        private static string AppendQueryParam(ref string urlSuffix, string name, string value)
            => urlSuffix += (urlSuffix.LastIndexOf('?') < 0 ? "?" : "&") + $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";

        /// <summary>
        /// Checks whether the URL is local.
        /// </summary>
        /// <remarks>The implementation is copied from https://github.com/aspnet/AspNetCore/blob/release/2.2/src/Mvc/Mvc.Core/src/Routing/UrlHelperBase.cs#L45 in order to provide the same behavior for local redirects.</remarks>
        public static bool IsLocalUrl(string url)
        {
            if (url.Length == 0)
            {
                return false;
            }

            // Check whether the URL contains only allowed characters
            if (!ContainsOnlyValidUrlChars(url))
            {
                return false;
            }

            // Allows "/" or "/foo" but not "//" or "/\".
            if (url[0] == '/')
            {
                // url is exactly "/"
                if (url.Length == 1)
                {
                    return true;
                }

                // url doesn't start with "//" or "/\"
                if (url[1] != '/' && url[1] != '\\')
                {
                    return true;
                }

                return false;
            }

            // Allows "~/" or "~/foo" but not "~//" or "~/\".
            if (url[0] == '~' && url.Length > 1 && url[1] == '/')
            {
                // url is exactly "~/"
                if (url.Length == 2)
                {
                    return true;
                }

                // url doesn't start with "~//" or "~/\"
                if (url[2] != '/' && url[2] != '\\')
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        private static bool ContainsOnlyValidUrlChars(string url)
        {
            for (int i = 0; i < url.Length; i++)
            {
                if ((url[i] < 'A' || url[i] > 'Z') && (url[i] < 'a' || url[i] > 'z') && (url[i] < '0' || url[i] > '9')
                    && url[i] != '-' && url[i] != '.' && url[i] != '_' && url[i] != '~' && url[i] != '%'
                    && url[i] != '!' && url[i] != '$' && url[i] != '&' && url[i] != '\'' && url[i] != '(' && url[i] != ')' && url[i] != '*' && url[i] != '+' && url[i] != ',' && url[i] != ';' && url[i] != '='
                    && url[i] != ':' && url[i] != '@' && url[i] != '/' && url[i] != '?')
                {
                    return false;
                }
            }
            return true;
        }
    }
}
