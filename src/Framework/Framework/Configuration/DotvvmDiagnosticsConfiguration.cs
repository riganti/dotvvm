using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmDiagnosticsConfiguration
    {
        /// <summary>
        /// Gets or sets the options of the compilation status page.
        /// </summary>
        [JsonProperty("compilationPage")]
        public DotvvmCompilationPageConfiguration CompilationPage
        {
            get { return _compilationPage; }
            set { ThrowIfFrozen(); _compilationPage = value; }
        }
        private DotvvmCompilationPageConfiguration _compilationPage = new();

        /// <summary>
        /// Gets or sets the options of the configuration status page.
        /// </summary>
        [JsonProperty("configurationPage")]
        public DotvvmConfigurationPageConfiguration ConfigurationPage
        {
            get { return _configurationPage; }
            set { ThrowIfFrozen(); _configurationPage = value; }
        }
        private DotvvmConfigurationPageConfiguration _configurationPage = new();

        private bool isFrozen = false;

        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error(nameof(DotvvmDiagnosticsConfiguration));
        }

        public void Freeze()
        {
            isFrozen = true;
            CompilationPage.Freeze();
            ConfigurationPage.Freeze();
        }

        public void Apply(DotvvmConfiguration config)
        {
            CompilationPage.Apply(config);
            ConfigurationPage.Apply(config);
        }
    }
}
