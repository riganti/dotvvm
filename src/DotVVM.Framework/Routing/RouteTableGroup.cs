using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Routing
{
    public class RouteTableGroup
    {
        public Action<string, RouteBase> AddToParentRouteTable { get; }

        public string GroupName { get; }

        public string RouteNamePrefix { get; }

        public string UrlPrefix { get; }

        public string VirtualPathPrefix { get; }

        public Func<IServiceProvider, IDotvvmPresenter> PresenterFactory { get; }

        public RouteTableGroup(string groupName,
            string routeNamePrefix,
            string urlPrefix,
            string virtualPathPrefix,
            Action<string, RouteBase> addToParentRouteTable,
            Func<IServiceProvider, IDotvvmPresenter> presenterFactory)
        {
            GroupName = groupName;
            RouteNamePrefix = routeNamePrefix;
            UrlPrefix = urlPrefix;
            VirtualPathPrefix = virtualPathPrefix;
            AddToParentRouteTable = addToParentRouteTable;
            PresenterFactory = presenterFactory;
        }
    }
}
