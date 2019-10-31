#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotVVM.Framework.Routing
{
    public class DotvvmRouteParser
    {
        private readonly IDictionary<string, IRouteParameterConstraint> routeConstraints;

        public DotvvmRouteParser(IDictionary<string, IRouteParameterConstraint> routeConstrains)
        {
            this.routeConstraints = routeConstrains;
        }

        public UrlParserResult ParseRouteUrl(string url, IDictionary<string, object?> defaultValues)
        {
            if (url.StartsWith("/", StringComparison.Ordinal))
                throw new ArgumentException("The route URL must not start with '/'!");
            if (url.EndsWith("/", StringComparison.Ordinal))
                throw new ArgumentException("The route URL must not end with '/'!");

            var regex = new StringBuilder("^");
            var parameters = new List<KeyValuePair<string, Func<string, ParameterParseResult>?>>();
            var urlBuilders = new List<Func<Dictionary<string, object?>, string>>();
            urlBuilders.Add(_ => "~");

            void AppendParameterParserResult(UrlParameterParserResult result)
            {
                regex.Append(result.ParameterRegexPart);
                parameters.Add(result.Parameter);
                urlBuilders.Add(result.UrlBuilder);
            }

            // We are always prepending '/' character to handle optional parameter at the start correctly
            url = '/' + url;

            // go through the route components and parse it
            var startIndex = 0;
            for (int i = 0; i < url.Length; i++)
            {
                if (url[i] == '/')
                {
                    // standard URL component
                    var str = url.Substring(startIndex, i - startIndex);
                    regex.Append(Regex.Escape(str));
                    urlBuilders.Add(_ => str);
                    startIndex = i;
                }
                else if (url[i] == '{')
                {
                    // route parameter
                    var str = url.Substring(startIndex, i - startIndex);
                    i++;
                    AppendParameterParserResult(ParseParameter(url, str, ref i, defaultValues));
                    startIndex = i + 1;
                }
            }
            if (startIndex < url.Length)
            {
                // standard URL component
                var str = url.Substring(startIndex);
                regex.Append(Regex.Escape(str));
                urlBuilders.Add(_ => str);
            }

            // finish the route-matching regular expression
            regex.Append("/?$");

            return new UrlParserResult
            {
                RouteRegex = new Regex(regex.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase),
                UrlBuilders = urlBuilders,
                Parameters = parameters
            };
        }

        private UrlParameterParserResult ParseParameter(string url, string prefix, ref int index, IDictionary<string, object?> defaultValues)
        {
            // find the end of the route parameter name
            var startIndex = index;
            index = url.IndexOfAny(new[] { '}', ':' }, index);
            if (index < 0)
            {
                throw new ArgumentException($"The route URL '{url}' is not valid! It contains an unclosed parameter.");
            }
            var name = url.Substring(startIndex, index - startIndex).Trim();

            // determine whether the parameter is optional - it must end with ?, or must be present in the DefaultValues collection
            var isOptional = name.EndsWith("?", StringComparison.Ordinal);
            if (isOptional)
            {
                name = name.Substring(0, name.Length - 1);
            }
            else
            {
                isOptional = defaultValues.ContainsKey(name);
            }

            // determine route parameter constraint
            IRouteParameterConstraint? type = null;
            string? parameter = null;
            if (url[index] == ':')
            {
                startIndex = index + 1;
                index = url.IndexOfAny("}(:".ToArray(), index + 1);
                if (index < 0)
                {
                    throw new ArgumentException($"The route URL '{url}' is not valid! It contains an unclosed parameter.");
                }

                var typeName = url.Substring(startIndex, index - startIndex);
                if (!routeConstraints.ContainsKey(typeName))
                {
                    throw new ArgumentException($"The route parameter constraint '{typeName}' is not valid!");
                }
                type = routeConstraints[typeName];
                if (url[index] == '(') // parameters
                {
                    index++;
                    startIndex = index;
                    int plevel = 0;
                    while (!(plevel == 0 && url[index] == ')'))
                    {
                        if (url[index] == '(') plevel++;
                        else if (url[index] == ')') plevel--;
                        index++;
                        if (url.Length == index) throw new AggregateException($"The route constraint parameter of '{name}:{type}' is not closed: {url}");
                    }
                    parameter = url.Substring(startIndex, index - startIndex);
                    index++;
                }
                if (url[index] == ':') throw new NotImplementedException("Support for multiple route constraints is not implemented."); // TODO

            }
            if (url[index] != '}') throw new AggregateException($"Route parameter { name } should be closed with curly bracket");

            Func<Dictionary<string, object?>, string> urlBuilder;
            // generate the URL builder
            if (isOptional)
            {
                urlBuilder = v =>
                {
                    if (v.TryGetValue(name, out var r) && r != null)
                    {
                        return prefix + r.ToString();
                    }
                    else
                    {
                        return "";
                    }
                };
            }
            else
            {
                urlBuilder = v => prefix + (v[name]?.ToString() ?? throw new ArgumentNullException($"Could not build route, parameter '{name}' is null"));
            }

            var parameterParser = type != null
                ? new KeyValuePair<string, Func<string, ParameterParseResult>?>(name, s => type.ParseString(s, parameter))
                : new KeyValuePair<string, Func<string, ParameterParseResult>?>(name, null);

            // generate the regex
            var pattern = type?.GetPartRegex(parameter) ?? "[^/]*?";     // parameters cannot contain /
            var result = $"{ Regex.Escape(prefix) }(?<param{Regex.Escape(name)}>{pattern})";
            if (isOptional)
            {
                result = "(" + result + ")?";
            }

            return new UrlParameterParserResult
            {
                ParameterRegexPart = result,
                UrlBuilder = urlBuilder,
                Parameter = parameterParser
            };
        }

        private struct UrlParameterParserResult
        {
            public string ParameterRegexPart { get; set; }
            public Func<Dictionary<string, object?>, string> UrlBuilder { get; set; }
            public KeyValuePair<string, Func<string, ParameterParseResult>?> Parameter { get; set; }
        }
    }

    public struct UrlParserResult
    {
        public Regex RouteRegex { get; set; }
        public List<Func<Dictionary<string, object?>, string>> UrlBuilders { get; set; }
        public List<KeyValuePair<string, Func<string, ParameterParseResult>?>> Parameters { get; set; }
    }
}
