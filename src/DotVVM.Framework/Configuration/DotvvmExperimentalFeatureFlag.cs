using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmExperimentalFeatureFlag
    {

        [JsonProperty("enabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Enabled { get; set; } = false;

        [JsonProperty("includedRoutes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HashSet<string> IncludedRoutes { get; private set; } = new HashSet<string>();

        [JsonProperty("excludedRoutes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HashSet<string> ExcludedRoutes { get; private set; } = new HashSet<string>();


        public void EnableForAllRoutes()
        {
            IncludedRoutes.Clear();
            ExcludedRoutes.Clear();
            Enabled = true;
        }

        public void EnableForRoutes(params string[] routes)
        {
            Enabled = false;
            ExcludedRoutes.Clear();

            IncludedRoutes.Clear();
            foreach (var route in routes)
            {
                IncludedRoutes.Add(route);
            }
        }

        public void EnableForAllRoutesExcept(params string[] routes)
        {
            Enabled = true;
            IncludedRoutes.Clear();

            ExcludedRoutes.Clear();
            foreach (var route in routes)
            {
                ExcludedRoutes.Add(route);
            }
        }

        public bool IsEnabledForAnyRoute()
        {
            return Enabled || IncludedRoutes.Count > 0;
        }

        public bool IsEnabledForRoute(string routeName)
        {
            return (Enabled && !ExcludedRoutes.Contains(routeName)) || (!Enabled && IncludedRoutes.Contains(routeName));
        }

    }

}
