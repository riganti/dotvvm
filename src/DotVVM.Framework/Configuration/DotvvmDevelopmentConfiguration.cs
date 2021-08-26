#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmDevelopmentConfiguration
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

        private bool isFrozen = false;

        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error(nameof(DotvvmDevelopmentConfiguration));
        }

        public void Freeze()
        {
            isFrozen = true;
        }

        public void Apply(DotvvmConfiguration config)
        {
            CompilationPage.Apply(config);
        }
    }
}
