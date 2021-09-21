using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmConfigurationPageConfiguration
    {
        public const string DefaultUrl = "_dotvvm/diagnostics/configuration";
        public const string DefaultRouteName = "_dotvvm_diagnostics_configuration";

        /// <summary>
        /// Gets or sets whether the configuration status page is enabled.
        /// </summary>
        /// <remarks>
        /// When null, the configuration page is automatically enabled if <see cref="DotvvmConfiguration.Debug"/>
        /// is true.
        /// </remarks>
        [JsonProperty("isEnabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public bool? IsEnabled
        {
            get { return _isEnabled; }
            set { ThrowIfFrozen(); _isEnabled = value; }
        }
        private bool? _isEnabled = null;

        /// <summary>
        /// Gets or sets the URL where the configuration page will be accessible from.
        /// </summary>
        [JsonProperty("url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(DefaultUrl)]
        public string Url
        {
            get { return _url; }
            set { ThrowIfFrozen(); _url = value; }
        }
        private string _url = DefaultUrl;

        /// <summary>
        /// Gets or sets the name of the route that the configuration page will be registered as.
        /// </summary>
        [JsonProperty("routeName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(DefaultRouteName)]
        public string RouteName
        {
            get { return _routeName; }
            set { ThrowIfFrozen(); _routeName = value; }
        }
        private string _routeName = DefaultRouteName;

        private bool isFrozen = false;

        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error(nameof(DotvvmConfigurationPageConfiguration));
        }

        public void Freeze()
        {
            isFrozen = true;
        }

        public void Apply(DotvvmConfiguration config)
        {
            if (IsEnabled == true || (IsEnabled == null && config.Debug))
            {
                config.RouteTable.Add(
                    routeName: RouteName,
                    url: Url,
                    virtualPath: "embedded://DotVVM.Framework/Diagnostics/ConfigurationPage.dothtml");
            }
        }
    }
}
