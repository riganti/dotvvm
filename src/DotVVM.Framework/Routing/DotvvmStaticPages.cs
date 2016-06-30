using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Routing
{
    public class DotvvmStaticPages
    {
        private readonly DotvvmConfiguration configuration;

        public Dictionary<string, string> RouteNamesToStorageUrls { get; }


        public DotvvmStaticPages(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
            this.RouteNamesToStorageUrls = new Dictionary<string, string>();
        }


        public void Add(string urlToStaticContentStorage, string routeName, string url, string virtualPath, object defaultValues = null, Func<IDotvvmPresenter> presenterFactory = null)
        {
            if (!Uri.IsWellFormedUriString(urlToStaticContentStorage, UriKind.Absolute)) throw new ArgumentException($"Parameter '{urlToStaticContentStorage}' is not valid Url.");

            this.RouteNamesToStorageUrls.Add(routeName, urlToStaticContentStorage);
            configuration.RouteTable.Add(routeName, url, virtualPath, defaultValues, presenterFactory);
        }


        public RouteBase this[string routeName]
        {
            get
            {
                if (!configuration.RouteTable.Contains(routeName)) throw new ArgumentException($"Route '{routeName}' have not been defined yet!");
                if (!RouteNamesToStorageUrls.ContainsValue(routeName)) throw new ArgumentException($"Route '{routeName}' is not defined in static routes!");

                return configuration.RouteTable.SingleOrDefault(rt => rt.RouteName == routeName);
            }
        }


        public bool Contains(string routeName)
        {
            return RouteNamesToStorageUrls.Any(r => string.Equals(r.Key, routeName, StringComparison.OrdinalIgnoreCase));
        }


        public string GetStaticStorage(string routeName)
        {
            return RouteNamesToStorageUrls.FirstOrDefault(k => k.Key == routeName).Value;
        }


        public string CreateAbsolutePathToStaticFile(DotvvmRequestContext currentContext)
        {
            var urlToStaticContentStorage = RouteNamesToStorageUrls.SingleOrDefault(rts => rts.Key == currentContext.Route.RouteName).Value;
            if (urlToStaticContentStorage == null) throw new Exception($"Current route '{currentContext.Route.RouteName}' is not static!");
            var urlAlias = CreateUrlAlias(currentContext);

            Uri storage = new Uri(urlToStaticContentStorage);
            Uri absoluteUri = new Uri(storage, urlAlias);
            return absoluteUri.ToString();
        }



        private static string CreateUrlAlias(DotvvmRequestContext currentContext)
        {
            var currentUrl = currentContext.Route.Url;
            currentUrl += ".html";
            var parameters = currentContext.Route.ParameterNames;

            foreach (var parameter in parameters)
            {
                currentUrl = currentUrl.Replace("{" + parameter + "}", currentContext.Parameters[parameter].ToString());
            }

            return currentUrl;
        }
    }
}
