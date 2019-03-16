using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DotVVM.Framework.Routing
{
    public class RouteTableGroup
    {
        [JsonIgnore]
        public Action<string, RouteBase> AddToParentRouteTable { get; private set; }

        public string GroupName { get; private set; }
        public string RouteNamePrefix { get; private set; }
        public string UrlPrefix { get; private set; }
        public string VirtualPathPrefix { get; private set; }

        public RouteTableGroup(string groupName, string routeNamePrefix, string urlPrefix, string virtualPathPrefix, Action<string, RouteBase> addToParentRouteTable)
        {
            GroupName = groupName;
            RouteNamePrefix = routeNamePrefix;
            UrlPrefix = urlPrefix;
            VirtualPathPrefix = virtualPathPrefix;
            AddToParentRouteTable = addToParentRouteTable;
        }
    }
}
