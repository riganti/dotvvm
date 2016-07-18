using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using System.Text.RegularExpressions;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Routing
{
    public class DotvvmRoute : RouteBase
    {
        private Func<IDotvvmPresenter> presenterFactory;

        private Regex routeRegex;
        private List<Func<Dictionary<string, object>, string>> urlBuilders;
        private List<KeyValuePair<string, Func<string, ParameterParseResult>>> parameters;

        /// <summary>
        /// Gets the names of the route parameters in the order in which they appear in the URL.
        /// </summary>
        public override IEnumerable<string> ParameterNames => parameters.Select(p => p.Key);


        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRoute"/> class.
        /// </summary>
        public DotvvmRoute(string url, string virtualPath, object defaultValues, Func<IDotvvmPresenter> presenterFactory, DotvvmConfiguration configuration)
            : base(url, virtualPath, defaultValues)
        {
            this.presenterFactory = presenterFactory;

            ParseRouteUrl(configuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRoute"/> class.
        /// </summary>
        public DotvvmRoute(string url, string virtualPath, IDictionary<string, object> defaultValues, Func<DotvvmPresenter> presenterFactory, DotvvmConfiguration configuration)
            : base(url, virtualPath, defaultValues)
        {
            this.presenterFactory = presenterFactory;

            ParseRouteUrl(configuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRoute"/> class.
        /// </summary>
        public DotvvmRoute(string url, string virtualPath, string name, IDictionary<string, object> defaultValues, Func<DotvvmPresenter> presenterFactory, DotvvmConfiguration configuration)
            : base(url, virtualPath, name, defaultValues)
        {
            this.presenterFactory = presenterFactory;

            ParseRouteUrl(configuration);
        }


        /// <summary>
        /// Parses the route URL and extracts the components.
        /// </summary>
        private void ParseRouteUrl(DotvvmConfiguration configuration)
        {
            if (Url.StartsWith("/", StringComparison.Ordinal))
                throw new ArgumentException("The route URL must not start with '/'!");
            if (Url.EndsWith("/", StringComparison.Ordinal))
                throw new ArgumentException("The route URL must not end with '/'!");

            parameters = new List<KeyValuePair<string, Func<string, ParameterParseResult>>>();

            urlBuilders = new List<Func<Dictionary<string, object>, string>>();
            urlBuilders.Add(_ => "~/");

            // go through the route components and parse it
            var regex = new StringBuilder("^");
            var startIndex = 0;
            for (int i = 0; i < Url.Length; i++)
            {
                if (Url[i] == '/')
                {
                    // standard URL component
                    var str = Url.Substring(startIndex, i - startIndex);
                    regex.Append(Regex.Escape(str));
                    urlBuilders.Add(_ => str);
                    startIndex = i;
                }
                else if (Url[i] == '{')
                {
                    // route parameter
                    var str = Url.Substring(startIndex, i - startIndex);
                    i++;
                    regex.Append(ParseParameter(str, ref i, configuration.RouteConstraints));
                    startIndex = i + 1;
                }
            }
            if (startIndex < Url.Length)
            {
                // standard URL component
                var str = Url.Substring(startIndex);
                regex.Append(Regex.Escape(str));
                urlBuilders.Add(_ => str);
            }

            // finish the route-matching regular expression
            regex.Append("/?$");
            routeRegex = new Regex(regex.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private string ParseParameter(string prefix, ref int index, Dictionary<string, IRouteParameterConstraint> routeConstraints)
        {
            // find the end of the route parameter name
            var startIndex = index;
            index = Url.IndexOfAny(new[] { '}', ':' }, index);
            if (index < 0)
            {
                throw new ArgumentException($"The route URL '{Url}' is not valid! It contains an unclosed parameter.");
            }
            var name = Url.Substring(startIndex, index - startIndex).Trim();

            // determine whether the parameter is optional - it must end with ?, or must be present in the DefaultValues collection
            var isOptional = name.EndsWith("?", StringComparison.Ordinal);
            if (isOptional)
            {
                name = name.Substring(0, name.Length - 1);
            }
            else
            {
                isOptional = DefaultValues.ContainsKey(name);
            }

            // determine route parameter constraint
            IRouteParameterConstraint type = null;
            string parameter = null;
            if (Url[index] == ':')
            {
                startIndex = index + 1;
                index = Url.IndexOfAny("}(:".ToArray(), index + 1);
                if (index < 0)
                {
                    throw new ArgumentException($"The route URL '{Url}' is not valid! It contains an unclosed parameter.");
                }

                var typeName = Url.Substring(startIndex, index - startIndex);
                if (!routeConstraints.ContainsKey(typeName))
                {
                    throw new ArgumentException($"The route parameter constraint '{typeName}' is not valid!");
                }
                type = routeConstraints[typeName];
                if (Url[index] == '(') // parameters
                {
                    index++;
                    startIndex = index;
                    int plevel = 0;
                    while (!(plevel == 0 && Url[index] == ')'))
                    {
                        if (Url[index] == '(') plevel++;
                        else if (Url[index] == ')') plevel--;
                        index++;
                        if (Url.Length == index) throw new AggregateException($"The route constraint parameter of '{name}:{type}' is not closed: {Url}");
                    }
                    parameter = Url.Substring(startIndex, index - startIndex);
                    index++;
                }
                if (Url[index] == ':') throw new NotImplementedException("Support for multiple route constraints is not implemented."); // TODO

            }
            if (Url[index] != '}') throw new AggregateException($"Route parameter { name } should be closed with curly bracket");

            // generate the URL builder
            if (isOptional)
            {
                urlBuilders.Add(v =>
                {
                    object r;
                    if (v.TryGetValue(name, out r))
                    {
                        return prefix + r.ToString();
                    }
                    else
                    {
                        return "";
                    }
                });
            }
            else
            {
                urlBuilders.Add(v => prefix + v[name].ToString());
            }

            // add a parameter
            if (type != null)parameters.Add(new KeyValuePair<string, Func<string, ParameterParseResult>>(name, s => type.ParseString(s, parameter)));
            else parameters.Add(new KeyValuePair<string, Func<string, ParameterParseResult>>(name, null));

            // generate the regex
            var pattern = type?.GetPartRegex(parameter) ?? "[^/]*?";     // parameters cannot contain /
            var result = $"{ Regex.Escape(prefix) }(?<param{Regex.Escape(name)}>{pattern})";
            if (isOptional)
            {
                result = "(" + result + ")?";
            }

            return result;
        }


        /// <summary>
        /// Determines whether the route matches to the specified URL and extracts the parameter values.
        /// </summary>
        public override bool IsMatch(string url, out IDictionary<string, object> values)
        {
            var match = routeRegex.Match(url);
            if (!match.Success)
            {
                values = null;
                return false;
            }

            values = new Dictionary<string, object>(DefaultValues, StringComparer.InvariantCultureIgnoreCase);

            foreach (var parameter in parameters)
            {
                var g = match.Groups["param" + parameter.Key];
                if (g.Success)
                {
                    if (parameter.Value != null)
                    {
                        var r = parameter.Value(g.Value);
                        if (!r.IsOK) return false;
                        values[parameter.Key] = r.Value;
                    }
                    else
                        values[parameter.Key] = g.Value;
                }
            }
            return true;
        }

        /// <summary>
        /// Builds the URL core from the parameters.
        /// </summary>
        protected override string BuildUrlCore(Dictionary<string, object> values)
        {
            try
            {
                return string.Concat(urlBuilders.Select(b => b(values)));
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not build URL for route '{ this.Url }' with values {{{ string.Join(", ", values.Select(kvp => kvp.Key + ": " + kvp.Value)) }}}", ex);
            }
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        public override Task ProcessRequest(DotvvmRequestContext context)
        {
            context.Presenter = presenterFactory();
            return context.Presenter.ProcessRequest(context);
        }
    }
}