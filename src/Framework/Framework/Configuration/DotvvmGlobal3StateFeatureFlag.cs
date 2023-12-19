using System;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    /// <summary> Overrides an automatically enabled feature by always enabling or disabling it for the entire application. </summary>
    public class DotvvmGlobal3StateFeatureFlag
    {
        [JsonIgnore]
        public string FlagName { get; }

        public DotvvmGlobal3StateFeatureFlag(string flagName)
        {
            FlagName = flagName;
        }

        /// <summary> Gets or sets whether the feature is enabled or disabled. </summary>
        [JsonProperty("enabled")]
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

        /// <summary> Resets the feature flag to its default state. </summary>
        public void Reset()
        {
            ThrowIfFrozen();
            Enabled = null;
        }

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
                throw FreezableUtils.Error($"{nameof(DotvvmGlobalFeatureFlag)} {this.FlagName}");
        }

        public void Freeze()
        {
            this.isFrozen = true;
        }

        public override string ToString() => $"Feature flag {FlagName}: {Enabled switch { null => "Default", true => "Enabled", false => "Disabled"}}";
    }
}
