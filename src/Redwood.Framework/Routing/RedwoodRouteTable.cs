using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Configuration;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Routing
{
    /// <summary>
    /// Represents the table of routes.
    /// </summary>
    public class RedwoodRouteTable : IEnumerable<RouteBase>
    {
        private readonly RedwoodConfiguration configuration;
        
        private List<KeyValuePair<string, RouteBase>> list = new List<KeyValuePair<string, RouteBase>>();


        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodRouteTable"/> class.
        /// </summary>
        public RedwoodRouteTable(RedwoodConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Creates the default presenter factory.
        /// </summary>
        public IRedwoodPresenter CreateDefaultPresenter()
        {
            return new RedwoodPresenter(
                new DefaultRedwoodViewBuilder(configuration),
                new DefaultViewModelLoader(),
                new ViewModelSerializer(configuration),
                new DefaultOutputRenderer()
            );
        }

        /// <summary>
        /// Adds the specified route name.
        /// </summary>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="url">The URL.</param>
        /// <param name="virtualPath">The virtual path of the RWHTML file.</param>
        /// <param name="defaultValues">The default values.</param>
        /// <param name="presenterFactory">The presenter factory.</param>
        public void Add(string routeName, string url, string virtualPath, object defaultValues, Func<IRedwoodPresenter> presenterFactory = null)
        {
            if (presenterFactory == null)
            {
                presenterFactory = CreateDefaultPresenter;
            }

            Add(routeName, new RedwoodRoute(url, virtualPath, defaultValues, presenterFactory));
        }

        /// <summary>
        /// Adds the specified name.
        /// </summary>
        public void Add(string routeName, RouteBase route)
        {
            list.Add(new KeyValuePair<string, RouteBase>(routeName, route));
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
