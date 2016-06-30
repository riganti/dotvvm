using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Routing
{
    public class DotvvmRouteStaticExtended : DotvvmRoute
    {
        public string StaticStorage;

        public DotvvmRouteStaticExtended(string staticStorage, string url, string virtualPath, object defaultValues, Func<IDotvvmPresenter> presenterFactory) : base(url, virtualPath, defaultValues, presenterFactory)
        {
            StaticStorage = staticStorage;
        }

        public DotvvmRouteStaticExtended(string staticStorage, string url, string virtualPath, IDictionary<string, object> defaultValues, Func<DotvvmPresenter> presenterFactory) : base(url, virtualPath, defaultValues, presenterFactory)
        {
            StaticStorage = staticStorage;
        }

        public DotvvmRouteStaticExtended(string staticStorage, string url, string virtualPath, string name, IDictionary<string, object> defaultValues, Func<DotvvmPresenter> presenterFactory) : base(url, virtualPath, name, defaultValues, presenterFactory)
        {
            StaticStorage = staticStorage;
        }

        protected override string BuildUrlCore(Dictionary<string, object> values)
        {
            try
            {
                var buildedUrl = string.Concat(urlBuilders.Select(b => b(values)));

                if (!string.IsNullOrWhiteSpace(StaticStorage))
                    buildedUrl = StaticStorage + buildedUrl.Substring(2) + ".html";

                return buildedUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not build url for route '{ this.Url }' with values {{{ string.Join(", ", values.Select(kvp => kvp.Key + ": " + kvp.Value)) }}}", ex);
            }
        }
    }
}
