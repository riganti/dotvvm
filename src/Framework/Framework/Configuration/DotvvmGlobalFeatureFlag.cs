using System;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.Configuration
{
    /// <summary> Enables or disables certain DotVVM feature for the entire application. </summary>
    public sealed class DotvvmGlobalFeatureFlag: IEquatable<DotvvmGlobalFeatureFlag>
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
        [JsonPropertyName("enabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
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
                throw FreezableUtils.Error($"{nameof(DotvvmGlobalFeatureFlag)} {this.FlagName}");
        }

        public void Freeze()
        {
            this.isFrozen = true;
        }

        public override string ToString() => $"Feature flag {FlagName}: {(Enabled ? "Enabled" : "Disabled")}";

        public bool Equals(DotvvmGlobalFeatureFlag? other) =>
            other is not null && this.Enabled == other.Enabled;
        public override bool Equals(object? obj) => obj is DotvvmGlobalFeatureFlag other && this.Equals(other);
        public override int GetHashCode() => throw new NotSupportedException("Use ReferenceEqualityComparer");
    }
}
