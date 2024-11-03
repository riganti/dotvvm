using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.Configuration
{
    /// <summary> Overrides an automatically enabled feature by always enabling or disabling it for the entire application or only for certain routes. </summary>
    public class Dotvvm3StateFeatureFlag: IDotvvmFeatureFlagAdditiveConfiguration, IEquatable<Dotvvm3StateFeatureFlag>
    {
        [JsonIgnore]
        public string FlagName { get; }

        public Dotvvm3StateFeatureFlag(string flagName)
        {
            FlagName = flagName;
        }

        /// <summary> Default state of this feature flag. true = enabled, false = disabled, null = enabled automatically based on other conditions (usually running in Development/Production environment) </summary>
        [JsonPropertyName("enabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool? Enabled
        {
            get => _enabled;
            set
            {
                ThrowIfFrozen();
                _enabled = value;
            }
        }
        private bool? _enabled = null;

        /// <summary> List of routes where the feature flag is always enabled. </summary>
        [JsonPropertyName("includedRoutes")]
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
        [JsonPropertyName("excludedRoutes")]
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

        /// <summary> Resets the feature flag to its default state. </summary>
        public IDotvvmFeatureFlagAdditiveConfiguration Reset()
        {
            ThrowIfFrozen();
            IncludedRoutes.Clear();
            ExcludedRoutes.Clear();
            Enabled = null;
            return this;
        }

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
            if (Enabled == true)
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
            if (Enabled == false)
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
        public bool IsEnabledForAnyRoute(bool defaultValue)
        {
            if (Enabled == false && IncludedRoutes.Count == 0)
                return false;
            return defaultValue || Enabled == true || IncludedRoutes.Count > 0;
        }

        /// <summary> Returns true/false if this feature flag has been explicitly enabled/disabled for the specified route. </summary>
        public bool? IsEnabledForRoute(string? routeName)
        {
            if (IncludedRoutes.Contains(routeName!))
                return true;
            if (ExcludedRoutes.Contains(routeName!))
                return false;
            return Enabled;
        }

        /// <summary> Returns if this feature flag has been explicitly for the specified route. </summary>
        public bool IsEnabledForRoute(string? routeName, bool defaultValue)
        {
            defaultValue = Enabled ?? defaultValue;
            if (defaultValue)
                return !ExcludedRoutes.Contains(routeName!);
            else
                return IncludedRoutes.Contains(routeName!);
        }

        private bool isFrozen = false;
        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error($"{nameof(DotvvmFeatureFlag)} {this.FlagName}");
        }
        public void Freeze()
        {
            this.isFrozen = true;
            FreezableSet.Freeze(ref this._excludedRoutes);
            FreezableSet.Freeze(ref this._includedRoutes);
        }

        public override string ToString()
        {
            var defaultStr = Enabled switch { null => "Default state", true => "Enabled by default", false => "Disabled by default" };
            var enabledStr = IncludedRoutes.Count > 0 ? $", enabled for routes: [{string.Join(", ", IncludedRoutes)}]" : null;
            var disabledStr = ExcludedRoutes.Count > 0 ? $", disabled for routes: [{string.Join(", ", ExcludedRoutes)}]" : null;
            return $"Feature flag {this.FlagName}: {defaultStr}{enabledStr}{disabledStr}";
        }

        public bool Equals(Dotvvm3StateFeatureFlag? other) =>
            other is not null &&
            this.Enabled == other.Enabled &&
            this.IncludedRoutes.SetEquals(other.IncludedRoutes) &&
            this.ExcludedRoutes.SetEquals(other.ExcludedRoutes);

        public override bool Equals(object? obj) => obj is Dotvvm3StateFeatureFlag other && Equals(other);
        public override int GetHashCode() => throw new NotSupportedException("Use ReferenceEqualityComparer");
    }
}
