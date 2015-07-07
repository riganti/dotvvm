using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Routing
{
    public class DotvvmRoute : RouteBase
    {

        private List<DotvvmRouteComponent> components;
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
            if (Url.StartsWith("/"))
                throw new ArgumentException("The route URL must not start with '/'!");
            
            var parts = Url.Split('/');
            if (parts.Length > 1 && parts.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException("The route URL must not end with '/' char or contain the sequence of '//'!");
            }

            components = parts.Select(p => new DotvvmRouteComponent(p)).ToList();
        }

        /// <summary>
        /// Gets the names of the route parameters in the order in which they appear in the URL.
        /// </summary>
        public override IEnumerable<string> ParameterNames
        {
            get { return components.Where(c => c.HasParameter).Select(c => c.ParameterName); }
        }

        /// <summary>
        /// Determines whether the route matches to the specified URL and extracts the parameter values.
        /// </summary>
        public override bool IsMatch(string url, out IDictionary<string, object> values)
        {
            values = new Dictionary<string, object>(DefaultValues);

            var parts = url.Split('/');
            if (parts.Length > components.Count)
            {
                // the URL has more components than the route, it does not match for sure
                return false;
            }

            for (int i = 0; i < components.Count; i++)
            {
                if (i < parts.Length)
                {
                    // compare the URL part and the route component
                    string value;
                    if (components[i].IsMatch(parts[i], out value))
                    {
                        if (components[i].HasParameter)
                        {
                            values[components[i].ParameterName] = value;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    // the URL finished and the route continues however if the route contains only optional parameters, we can match if
                    if (components[i].HasPrefix || components[i].HasSuffix)
                    {
                        return false;
                    }
                    if (!components[i].HasParameter || !DefaultValues.ContainsKey(components[i].ParameterName))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Builds the URL core from the parameters.
        /// </summary>
        protected override string BuildUrlCore(Dictionary<string, object> values)
        {
            var stringBuilder = new StringBuilder("~/");

            var isFirst = true;
            foreach (var component in components)
            {
                if (!isFirst)
                {
                    stringBuilder.Append('/');
                }
                isFirst = false;

                component.BuildUrl(stringBuilder, values);
            }

            return stringBuilder.ToString();
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