using System;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    /// <summary> Enables or disables certain DotVVM feature for the entire application. </summary>
    public class DotvvmGlobalFeatureFlag
    {
        [JsonIgnore]
        public string FlagName { get; }

        public DotvvmGlobalFeatureFlag(string flagName)
        {
            FlagName = flagName;
        }

        [Obsolete("Please specify the feature flag name")]
        public DotvvmGlobalFeatureFlag(): this("Unknown") { }

        /// <summary> Gets or sets whether the feature is enabled or disabled. </summary>
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

        /// <summary> Enables the feature for this application. </summary>
        public void Enable()
        {
            ThrowIfFrozen();
            Enabled = true;
        }
        /// <summary> Disables the feature for this application </summary>
        public void Disable()
        {
            ThrowIfFrozen();
            Enabled = false;
        }

        private bool isFrozen = false;
        private void ThrowIfFrozen()
        {
            if (isFrozen)
                FreezableUtils.Error($"{nameof(DotvvmGlobalFeatureFlag)} {this.FlagName}");
        }

        public void Freeze()
        {
            this.isFrozen = true;
        }

        public override string ToString() => $"Feature flag {FlagName}: {(Enabled ? "Enabled" : "Disabled")}";
    }
}
