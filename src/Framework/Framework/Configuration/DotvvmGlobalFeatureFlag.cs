using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmGlobalFeatureFlag
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

        public void Enable()
        {
            ThrowIfFrozen();
            Enabled = true;
        }
        public void Disable()
        {
            ThrowIfFrozen();
            Enabled = false;
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
        }
    }
}
