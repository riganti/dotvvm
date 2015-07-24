using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Security;

namespace DotVVM.Framework.Routing
{
    /// <summary>
    /// Represents the table of routes.
    /// </summary>
    public class DotvvmRouteTable : IEnumerable<RouteBase>
    {
        private readonly DotvvmConfiguration configuration;
        
        private List<KeyValuePair<string, RouteBase>> list = new List<KeyValuePair<string, RouteBase>>();


        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRouteTable"/> class.
        /// </summary>
        public DotvvmRouteTable(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Creates the default presenter factory.
        /// </summary>
        public IDotvvmPresenter GetDefaultPresenter()
        {
            return configuration.ServiceLocator.GetService<IDotvvmPresenter>();
        }

        /// <summary>
        /// Adds the specified route name.
        /// </summary>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="url">The URL.</param>
        /// <param name="virtualPath">The virtual path of the Dothtml file.</param>
        /// <param name="defaultValues">The default values.</param>
        /// <param name="presenterFactory">The presenter factory.</param>
        public void Add(string routeName, string url, string virtualPath, object defaultValues, Func<IDotvvmPresenter> presenterFactory = null)
        {
            if (presenterFactory == null)
            {
                presenterFactory = GetDefaultPresenter;
            }

            Add(routeName, new DotvvmRoute(url, virtualPath, defaultValues, presenterFactory));
        }

        /// <summary>
        /// Adds the specified name.
        /// </summary>
        public void Add(string routeName, RouteBase route)
        {
            list.Add(new KeyValuePair<string, RouteBase>(routeName, route));
        }

        public RouteBase this[string key]
        {
            get
            {
                var route = list.FirstOrDefault(i => i.Key == key).Value;
                if (route == null)
                {
                    throw new ArgumentException(string.Format("The route with name {0} does not exist!", key));
                }
                return route;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<RouteBase> GetEnumerator()
        {
            return list.Select(l => l.Value).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
