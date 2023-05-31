using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    /// <summary> Enables or disables certain DotVVM feature for the entire application or only for certain routes. </summary>
    public class DotvvmFeatureFlag: IDotvvmFeatureFlagAdditiveConfiguration
    {
        [JsonIgnore]
        public string FlagName { get; }

        public DotvvmFeatureFlag(string flagName, bool enabled = false)
        {
            FlagName = flagName;
            Enabled = enabled;
        }

        [Obsolete("Please specify the feature flag name")]
        public DotvvmFeatureFlag(): this("Unknown")
        {
        }

        /// <summary> Gets or set the default state of this feature flag. If the current route doesn't match any <see cref="IncludedRoutes" /> or <see cref="ExcludedRoutes" />, it will  </summary>
        [JsonProperty("enabled")]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                ThrowIfFrozen();
                _enabled = value;
            }
        }
        private bool _enabled = false;

        /// <summary> List of routes where the feature flag is always enabled. </summary>
        [JsonProperty("includedRoutes")]
        public ISet<string> IncludedRoutes
        {
            get => _includedRoutes;
            set
            {
                ThrowIfFrozen();
                _includedRoutes = value;
            }
        }
        private ISet<string> _includedRoutes = new FreezableSet<string>(comparer: StringComparer.OrdinalIgnoreCase);

        /// <summary> List of routes where the feature flag is always disabled. </summary>
        [JsonProperty("excludedRoutes")]
        public ISet<string> ExcludedRoutes
        {
            get => _excludedRoutes;
            set
            {
                ThrowIfFrozen();
                _excludedRoutes = value;
            }
        }
        private ISet<string> _excludedRoutes = new FreezableSet<string>(comparer: StringComparer.OrdinalIgnoreCase);

        /// <summary> Enables the feature flag for all routes, even if it has been previously disabled. </summary>
        public IDotvvmFeatureFlagAdditiveConfiguration EnableForAllRoutes()
        {
            ThrowIfFrozen();
            IncludedRoutes.Clear();
            ExcludedRoutes.Clear();
            Enabled = true;
            return this;
        }

        /// <summary> Disables the feature flag for all routes, even if it has been previously enabled. </summary>
        public IDotvvmFeatureFlagAdditiveConfiguration DisableForAllRoutes()
        {
            ThrowIfFrozen();
            IncludedRoutes.Clear();
            ExcludedRoutes.Clear();
            Enabled = false;
            return this;
        }

        /// <summary> Enables the feature flag only for the specified routes, and disables for all other (Clears any previous rules). </summary>
        public void EnableForRoutes(params string[] routes)
        {
            ThrowIfFrozen();
            Enabled = false;
            ExcludedRoutes.Clear();

            IncludedRoutes.Clear();
            foreach (var route in routes)
            {
                IncludedRoutes.Add(route);
            }
        }

        /// <summary> Enables the feature flag for all routes except the specified ones (Clears any previous rules). </summary>
        public void EnableForAllRoutesExcept(params string[] routes)
        {
            ThrowIfFrozen();
            Enabled = true;
            IncludedRoutes.Clear();

            ExcludedRoutes.Clear();
            foreach (var route in routes)
            {
                ExcludedRoutes.Add(route);
            }
        }

        /// <summary> Include the specified route in this feature flag. Enables the feature for the route. </summary>
        public IDotvvmFeatureFlagAdditiveConfiguration IncludeRoute(string routeName)
        {
            ThrowIfFrozen();
            if (Enabled)
                throw new InvalidOperationException($"Cannot include route '{routeName}' because the feature flag {this.FlagName} is enabled by default.");
            if (ExcludedRoutes.Contains(routeName))
                throw new InvalidOperationException($"Cannot include route '{routeName}' because it is already in the list of excluded routes.");
            IncludedRoutes.Add(routeName);
            return this;
        }
        /// <summary> Include the specified routes in this feature flag. Enables the feature for the routes. </summary>
        public IDotvvmFeatureFlagAdditiveConfiguration IncludeRoutes(params string[] routeNames)
        {
            foreach (var routeName in routeNames)
            {
                IncludeRoute(routeName);
            }
            return this;
        }
        /// <summary> Exclude the specified route from this feature flag. Disables the feature for the route. </summary>
        public IDotvvmFeatureFlagAdditiveConfiguration ExcludeRoute(string routeName)
        {
            ThrowIfFrozen();
            if (!Enabled)
                throw new InvalidOperationException($"Cannot exclude route '{routeName}' because the feature flag {this.FlagName} is disabled by default.");
            if (IncludedRoutes.Contains(routeName))
                throw new InvalidOperationException($"Cannot exclude route '{routeName}' because it is already in the list of included routes.");
            ExcludedRoutes.Add(routeName);
            return this;
        }
        /// <summary> Exclude the specified routes from this feature flag. Disables the feature for the routes. </summary>
        public IDotvvmFeatureFlagAdditiveConfiguration ExcludeRoutes(params string[] routeNames)
        {
            foreach (var routeName in routeNames)
            {
                ExcludeRoute(routeName);
            }
            return this;
        }

        /// <summary> Return true if there exists a route where this feature flag is enabled. </summary>
        public bool IsEnabledForAnyRoute()
        {
            return Enabled || IncludedRoutes.Count > 0;
        }

        /// <summary> Return true if this feature flag is enabled for the specified route. </summary>
        public bool IsEnabledForRoute(string? routeName)
        {
            return (Enabled && !ExcludedRoutes.Contains(routeName!)) || (!Enabled && IncludedRoutes.Contains(routeName!));
        }

        private bool isFrozen = false;
        private void ThrowIfFrozen()
        {
            if (isFrozen)
                FreezableUtils.Error($"{nameof(DotvvmFeatureFlag)} {this.FlagName}");
        }
        public void Freeze()
        {
            this.isFrozen = true;
            FreezableSet.Freeze(ref this._excludedRoutes);
            FreezableSet.Freeze(ref this._includedRoutes);
        }

        public override string ToString()
        {
            var exceptIn = Enabled ? ExcludedRoutes : IncludedRoutes;
            var exceptInStr = exceptIn.Count > 0 ? $", except in {string.Join(", ", exceptIn)}" : "";
            return $"Feature flag {this.FlagName}: {(Enabled ? "Enabled" : "Disabled")}{exceptInStr}";
        }
    }

    public interface IDotvvmFeatureFlagAdditiveConfiguration
    {
        /// <summary> Include the specified route in this feature flag. Enables the feature for the route. </summary>
        IDotvvmFeatureFlagAdditiveConfiguration IncludeRoute(string routeName);
        /// <summary> Include the specified routes in this feature flag. Enables the feature for the routes. </summary>
        IDotvvmFeatureFlagAdditiveConfiguration IncludeRoutes(params string[] routeNames);
        /// <summary> Exclude the specified route from this feature flag. Disables the feature for the route. </summary>
        IDotvvmFeatureFlagAdditiveConfiguration ExcludeRoute(string routeName);
        /// <summary> Exclude the specified routes from this feature flag. Disables the feature for the routes. </summary>
        IDotvvmFeatureFlagAdditiveConfiguration ExcludeRoutes(params string[] routeNames);
    }
}
