using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using System.Text.RegularExpressions;

namespace DotVVM.Framework.Routing
{
    public class DotvvmRoute : RouteBase
    {
        public static readonly Dictionary<string, IRouteParameterType> ParameterTypes = new Dictionary<string, IRouteParameterType>
        {
            { "int", new GenericRouteParameterType("-?[0-9]*?", s => int.Parse(s)) },
            { "posint", new GenericRouteParameterType("[0-9]*?", s => int.Parse(s)) },
            { "float", new GenericRouteParameterType("-?[0-9.e]*?", s => float.Parse(s)) },
            { "guid", new GenericRouteParameterType("[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", s => Guid.Parse(s)) }
        };

        private Func<IDotvvmPresenter> presenterFactory;
        
        private Regex routeRegex;
        private List<Func<Dictionary<string, object>, string>> urlBuilders;
        private List<KeyValuePair<string, IRouteParameterType>> parameters;

        /// <summary>
        /// Gets the names of the route parameters in the order in which they appear in the URL.
        /// </summary>
        public override IEnumerable<string> ParameterNames => parameters.Select(p => p.Key);


        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRoute"/> class.
        /// </summary>
        public DotvvmRoute(string url, string virtualPath, object defaultValues, Func<IDotvvmPresenter> presenterFactory)
            : base(url, virtualPath, defaultValues)
        {
            this.presenterFactory = presenterFactory;

            ParseRouteUrl();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRoute"/> class.
        /// </summary>
        public DotvvmRoute(string url, string virtualPath, IDictionary<string, object> defaultValues, Func<DotvvmPresenter> presenterFactory)
            : base(url, virtualPath, defaultValues)
        {
            this.presenterFactory = presenterFactory;

            ParseRouteUrl();
        }


        /// <summary>
        /// Parses the route URL and extracts the components.
        /// </summary>
        private void ParseRouteUrl()
        {
            if (Url.StartsWith("/", StringComparison.Ordinal))
                throw new ArgumentException("The route URL must not start with '/'!");
            if (Url.EndsWith("/", StringComparison.Ordinal))
                throw new ArgumentException("The route URL must not end with '/'!");

            parameters = new List<KeyValuePair<string, IRouteParameterType>>();
            
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
                    regex.Append(ParseParameter(str, ref i));
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

        private string ParseParameter(string prefix, ref int index)
        {
            // find the end of the route parameter name
            var startIndex = index;
            index = Url.IndexOfAny(new[] {'}', ':'}, index);
            if (index < 0)
            {
                throw new ArgumentException($"The route URL '{Url}' is not valid! It contains an unclosed parameter.");
            }
            var name = Url.Substring(startIndex, index - startIndex).Trim();
            
            // determine whether the parameter is optional - it must end with ?, or must be present in the DefaultValues collection
            var isOptional = name.EndsWith("?");
            if (isOptional)
            {
                name = name.Substring(0, name.Length - 1);
            }
            else
            {
                isOptional = DefaultValues.ContainsKey(name);
            }

            // determine route parameter constraint
            IRouteParameterType type = null;
            if (Url[index] == ':')
            {
                startIndex = index + 1;
                index = Url.IndexOf('}');
                if (index < 0)
                {
                    throw new ArgumentException($"The route URL '{Url}' is not valid! It contains an unclosed parameter.");
                }

                var typeName = Url.Substring(startIndex, index - startIndex);
                if (!ParameterTypes.ContainsKey(typeName))
                {
                    throw new ArgumentException($"The route parameter constraint '{typeName}' is not valid!");
                }
                type = ParameterTypes[typeName];
            }

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
            parameters.Add(new KeyValuePair<string, IRouteParameterType>(name, type));

            // generate the regex
            var pattern = type?.GetPartRegex() ?? "[^/]*?";     // parameters cannot contain /
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
                        values[parameter.Key] = parameter.Value.ParseString(g.Value);
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
                throw new Exception($"Could not build url for route '{ this.Url }' with values {{{ string.Join(", ", values.Select(kvp => kvp.Key + ": " + kvp.Value)) }}}", ex);
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