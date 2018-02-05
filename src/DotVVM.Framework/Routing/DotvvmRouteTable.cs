using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection;

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
        /// Dictionary for faster checking of duplicate entries when adding.
        /// </summary>
        private Dictionary<string, RouteBase> dictionary = new Dictionary<string, RouteBase>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Dictionary for groups of RouteTables.
        /// </summary>
        private Dictionary<string, DotvvmRouteTable> routeTableGroups = new Dictionary<string, DotvvmRouteTable>();

        /// <summary>
        /// Contains information about the group of this RouteTable.
        /// </summary>
        private RouteTableGroup group = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRouteTable"/> class.
        /// </summary>
        public DotvvmRouteTable(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Returns RouteTable of specific group name.
        /// </summary>
        /// <param name="groupName">Name of the group</param>
        /// <returns></returns>
        public DotvvmRouteTable GetGroup(string groupName)
        {
            if (groupName == null)
                throw new ArgumentNullException("Name of the group cannot be null!");
            var group = routeTableGroups[groupName];
            return group;
        }

        /// <summary>
        /// Adds a group of routes
        /// </summary>
        /// <param name="groupName">Name of the group</param>
        /// <param name="urlPrefix">Url prefix of added routes</param>
        /// <param name="virtualPathPrefix">Virtual path prefix of added routes</param>
        /// <param name="content">Contains routes to be added</param>
        public void AddGroup(string groupName, string urlPrefix, string virtualPathPrefix, Action<DotvvmRouteTable> content)
        {
            if (groupName == null || groupName == "")
                throw new ArgumentNullException("Name of the group cannot be null or empty!");
            if (routeTableGroups.ContainsKey(groupName))
            {
                throw new InvalidOperationException($"The group with name '{groupName}' has already been registered!");
            }
            urlPrefix = group?.UrlPrefix + ((urlPrefix == null || urlPrefix == "") ? "" : urlPrefix + "/");
            virtualPathPrefix = group?.VirtualPathPrefix + ((virtualPathPrefix == null || virtualPathPrefix == "") ? "" : virtualPathPrefix + "/");
            var newGroup = new DotvvmRouteTable(configuration);
            newGroup.group = new RouteTableGroup(groupName, group?.RouteNamePrefix + groupName + "_", urlPrefix, virtualPathPrefix, Add);

            content(newGroup);
            routeTableGroups.Add(groupName, newGroup);
        }

        /// <summary>
        /// Creates the default presenter factory.
        /// </summary>
        public IDotvvmPresenter GetDefaultPresenter(IServiceProvider provider) =>
            provider.GetRequiredService<IDotvvmPresenter>();

        /// <summary>
        /// Adds the specified route name.
        /// </summary>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="url">The URL.</param>
        /// <param name="virtualPath">The virtual path of the Dothtml file.</param>
        /// <param name="defaultValues">The default values.</param>
        public void Add(string routeName, string url, string virtualPath, object defaultValues = null, Func<IServiceProvider, IDotvvmPresenter> presenterFactory = null)
        {
            Add(group?.RouteNamePrefix + routeName, new DotvvmRoute(group?.UrlPrefix + url, group?.VirtualPathPrefix + virtualPath, defaultValues, presenterFactory ?? GetDefaultPresenter, configuration));
        }


        /// <summary>
        /// Adds the specified route name.
        /// </summary>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="url">The URL.</param>
        /// <param name="defaultValues">The default values.</param>
        /// <param name="presenterFactory">The presenter factory.</param>
        public void Add(string routeName, string url, Func<IServiceProvider, IDotvvmPresenter> presenterFactory, object defaultValues = null)
        {

            Add(group?.RouteNamePrefix + routeName, new DotvvmRoute(group?.UrlPrefix + url, group?.VirtualPathPrefix, defaultValues, presenterFactory, configuration));
        }


        /// <summary>
        /// Adds the specified route name.
        /// </summary>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="url">The URL.</param>
        /// <param name="presenterType">The presenter factory.</param>
        /// <param name="defaultValues">The default values.</param>
        public void Add(string routeName, string url, Type presenterType, object defaultValues = null)
        {
            if (!typeof(IDotvvmPresenter).IsAssignableFrom(presenterType))
            {
                throw new ArgumentException($@"{nameof(presenterType)} has to inherit from DotVVM.Framework.Hosting.IDotvvmPresenter.", nameof(presenterType));
            }
            Func<IServiceProvider, IDotvvmPresenter> presenterFactory = provider => (IDotvvmPresenter)provider.GetService(presenterType);
            Add(routeName, url, presenterFactory, defaultValues);
        }

        /// <summary>
        /// Adds the specified name.
        /// </summary>
        public void Add(string routeName, RouteBase route)
        {
            if (dictionary.ContainsKey(routeName))
            {
                throw new InvalidOperationException($"The route with name '{routeName}' has already been registered!");
            }
            // internal assign routename
            route.RouteName = routeName;

            group?.AddToParentRouteTable?.Invoke(routeName, route);

            // The list is used for finding the routes because it keeps the ordering, the dictionary is for checking duplicates
            list.Add(new KeyValuePair<string, RouteBase>(routeName, route));
            dictionary.Add(routeName, route);
        }

        public bool Contains(string routeName)
        {
            return dictionary.ContainsKey(routeName);
        }

        public RouteBase this[string routeName]
        {
            get
            {
                if (!dictionary.TryGetValue(routeName, out var route))
                {
                    throw new ArgumentException($"The route with name '{routeName}' does not exist!");
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
