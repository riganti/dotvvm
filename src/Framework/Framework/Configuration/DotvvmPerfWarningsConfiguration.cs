using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmPerfWarningsConfiguration
    {
        /// <summary> Gets or sets whether the warnings about potentially bad performance are enabled. By default, it enabled in both Debug and Production environments.
        /// Before turning it off, we suggest tweaking the warning thresholds if you find the default values to be too noisy. </summary>
        [JsonProperty("isEnabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(true)]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { ThrowIfFrozen(); _isEnabled = value; }
        }
        private bool _isEnabled = true;

        /// <summary> Gets or sets the threshold for the warning about too slow requests. In seconds, by default it's 3 seconds. </summary>
        [JsonProperty("slowRequestSeconds", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(3.0)]
        public double SlowRequestSeconds
        {
            get { return _slowRequestSeconds; }
            set { ThrowIfFrozen(); _slowRequestSeconds = value; }
        }
        private double _slowRequestSeconds = 3.0;


        /// <summary> Gets or sets the threshold for the warning about too big viewmodels. In bytes, by default it's 5 megabytes. </summary>
        [JsonProperty("bigViewModelBytes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(5 * 1024 * 1024)]
        public double BigViewModelBytes
        {
            get { return _bigViewModelBytes; }
            set { ThrowIfFrozen(); _bigViewModelBytes = value; }
        }
        private double _bigViewModelBytes = 5 * 1024 * 1024;

        private bool isFrozen = false;
        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error(nameof(DotvvmCompilationPageConfiguration));
        }
        public void Freeze()
        {
            isFrozen = true;
        }
    }
}
