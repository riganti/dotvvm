using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    /// <summary> Enables certain DotVVM feature for the entire application or only for certain routes. </summary>
    public class DotvvmFeatureFlag
    {

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
        private ISet<string> _includedRoutes = new FreezableSet<string>();

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
        private ISet<string> _excludedRoutes = new FreezableSet<string>();

        public void EnableForAllRoutes()
        {
            ThrowIfFrozen();
            IncludedRoutes.Clear();
            ExcludedRoutes.Clear();
            Enabled = true;
        }

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

        public bool IsEnabledForAnyRoute()
        {
            return Enabled || IncludedRoutes.Count > 0;
        }

        public bool IsEnabledForRoute(string? routeName)
        {
            return (Enabled && !ExcludedRoutes.Contains(routeName!)) || (!Enabled && IncludedRoutes.Contains(routeName!));
        }

        private bool isFrozen = false;
        private void ThrowIfFrozen()
        {
            if (isFrozen)
                FreezableUtils.Error(nameof(DotvvmFeatureFlag));
        }
        public void Freeze()
        {
            this.isFrozen = true;
            FreezableSet.Freeze(ref this._excludedRoutes);
            FreezableSet.Freeze(ref this._includedRoutes);
        }
    }
}
