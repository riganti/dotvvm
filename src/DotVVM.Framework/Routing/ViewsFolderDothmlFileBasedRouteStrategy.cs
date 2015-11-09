using DotVVM.Framework.Configuration;
using System;
using System.Security.Policy;

namespace DotVVM.Framework.Routing
{
    public class ViewsFolderDothmlFileBasedRouteStrategy : FolderDothmlFileBasedRouteStrategy
    {
        public ViewsFolderDothmlFileBasedRouteStrategy(DotvvmConfiguration configuration, bool tryDetermineDefaultRoute = true) : base(configuration, "Views", tryDetermineDefaultRoute)
        {
        }
    }
}