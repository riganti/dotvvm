using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmDiagnosticsConfiguration
    {
        /// <summary>
        /// Gets or sets the options of the compilation status page.
        /// </summary>
        [JsonPropertyName("compilationPage")]
        public DotvvmCompilationPageConfiguration CompilationPage
        {
            get { return _compilationPage; }
            set { ThrowIfFrozen(); _compilationPage = value; }
        }
        private DotvvmCompilationPageConfiguration _compilationPage = new();

        /// <summary>
        /// Gets or sets the options for runtime warning about slow requests, too big viewmodels, ...
        /// </summary>
        [JsonPropertyName("perfWarnings")]
        public DotvvmPerfWarningsConfiguration PerfWarnings
        {
            get { return _perfWarnings; }
            set { ThrowIfFrozen(); _perfWarnings = value; }
        }
        private DotvvmPerfWarningsConfiguration _perfWarnings = new();

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
            PerfWarnings.Freeze();
        }

        public void Apply(DotvvmConfiguration config)
        {
            CompilationPage.Apply(config);
        }
    }
}
