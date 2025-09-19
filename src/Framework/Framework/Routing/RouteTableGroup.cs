using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Routing
{
    public class RouteTableGroup
    {
        private readonly FreezableDictionary<object, object> extensionData = new();
        /// <summary>
        /// Contains additional metadata about the route group.
        /// </summary>
        public IDictionary<object, object> ExtensionData => extensionData;

        public Action<RouteBase> AddToParentRouteTable { get; private set; }

        public Func<IServiceProvider, IDotvvmPresenter>? PresenterFactory { get; }

        public string GroupName { get; private set; }
        public string RouteNamePrefix { get; private set; }
        public string UrlPrefix { get; private set; }
        public string VirtualPathPrefix { get; private set; }
        public ImmutableArray<LocalizedRouteUrl>? LocalizedUrls { get; private set; }

        /// <summary>
        /// Gets the parent route group that contains this route.
        /// </summary>
        public RouteTableGroup? ParentRouteGroup { get; internal set; }

        public RouteTableGroup(string groupName, string routeNamePrefix, string urlPrefix, string virtualPathPrefix, Action<RouteBase> addToParentRouteTable, Func<IServiceProvider, IDotvvmPresenter>? presenterFactory, LocalizedRouteUrl[]? localizedUrls = null)
        {
            GroupName = groupName;
            RouteNamePrefix = routeNamePrefix;
            UrlPrefix = urlPrefix;
            VirtualPathPrefix = virtualPathPrefix;
            AddToParentRouteTable = addToParentRouteTable;
            PresenterFactory = presenterFactory;
            LocalizedUrls = localizedUrls?.ToImmutableArray();
        }

        public void Freeze()
        {
            extensionData.Freeze();
        }
    }
}
