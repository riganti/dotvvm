using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DotVVM.Framework.Diagnostics
{
    internal class DotvvmDiagnosticsConfiguration
    {
        private DiagnosticsServerConfiguration configuration = null;

        internal string DiagnosticsServerHostname
        {
            get
            {
                if (configuration == null)
                    LoadConfiguration();
                return configuration.HostName;
            }
        }

        public bool IsLoaded => configuration != null;

        public void LoadConfiguration()
        {
            try
            {
                var diagnosticsJson = File.ReadAllText(DiagnosticsServerConfiguration.DiagnosticsFilePath);
                configuration = JsonConvert.DeserializeObject<DiagnosticsServerConfiguration>(diagnosticsJson);
            }
            catch
            {
                // ignored
            }
        }

        internal int? DiagnosticsServerPort
        {
            get
            {
                if (configuration == null)
                    LoadConfiguration();
                return configuration?.Port;
            }
        }

    }
}
