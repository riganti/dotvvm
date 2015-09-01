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

        private Regex routeRegex;
        private List<Func<Dictionary<string, object>, string>> urlBuilders;
        private Func<IDotvvmPresenter> presenterFactory;


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

            urlBuilders = new List<Func<Dictionary<string, object>, string>>();
            parameters = new List<KeyValuePair<string, IRouteParameterType>>();
            var regex = new StringBuilder("^");
            var startIndex = 0;
            for (int i = 0; i < Url.Length; i++)
            {
                if (Url[i] == '/')
                {
                    var str = Url.Substring(startIndex, i - startIndex);
                    regex.Append(Regex.Escape(str));
                    startIndex = i;
                }
                else if (Url[i] == '{')
                {
                    var str = Url.Substring(startIndex, i - startIndex);
                    regex.Append(ParseParameter(str, ref i));
                    startIndex = i + 1;
                }
            }
            regex.Append(Regex.Escape(Url.Substring(startIndex)));

            regex.Append("/?$");
            routeRegex = new Regex(regex.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        string ParseParameter(string prefix, ref int index)
        {
            index++;
            var startIndex = index;
            while (index < Url.Length && Url[index] != '}' && Url[index] != ':') index++;
            var name = Url.Substring(startIndex, index - startIndex).Trim();
            var nullable = name.Last() == '?';
            IRouteParameterType type = null;

            if (Url[index] == ':')
            {
                index++;
                startIndex = index;
                while (index < Url.Length && Url[index] != '}') index++;
                var typeName = Url.Substring(startIndex, index - startIndex);
                type = ParameterTypes[typeName];
            }

            if (nullable)
            {
                name = name.Remove(name.Length - 1);
                urlBuilders.Add(v => { object r; if (v.TryGetValue(name, out r)) return prefix + r.ToString(); else return ""; });
            }
            else
            {
                urlBuilders.Add(v => v[name].ToString());
            }
            var pattern = type?.GetPartRegex() ?? ".*?";
            parameters.Add(new KeyValuePair<string, IRouteParameterType>(name, type));
            var result = $"{ Regex.Escape(prefix) }(?<param{Regex.Escape(name)}>{pattern})";
            if (nullable) return "(" + result + ")?";
            else return result;
        }
        private List<KeyValuePair<string, IRouteParameterType>> parameters;
        /// <summary>
        /// Gets the names of the route parameters in the order in which they appear in the URL.
        /// </summary>
        public override IEnumerable<string> ParameterNames => parameters.Select(p => p.Key);

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

            values = new Dictionary<string, object>(DefaultValues);

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
            return string.Concat(urlBuilders.Select(b => b(values)));
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