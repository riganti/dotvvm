using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DotVVM.Framework.Diagnostics
{

    public class DotvvmDiagnosticsConfiguration
    {
        public DotvvmDiagnosticsConfiguration()
        {
            LoadConfiguration();
        }

        private DiagnosticsServerConfiguration configuration;

        public string DiagnosticsServerHostname
        {
            get
            {
                if (configuration == null)
                    LoadConfiguration();
                return configuration.HostName;
            }
        }

        public int? DiagnosticsServerPort
        {
            get
            {
                if (configuration == null)
                    LoadConfiguration();
                return configuration?.Port;
            }
        }

        private void LoadConfiguration()
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
    }
}
