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

        public string DiagnosticsServerHostname => configuration?.HostName;

        public int? DiagnosticsServerPort => configuration?.Port;

        private void LoadConfiguration()
        {
            try
            {
                var path = DiagnosticsServerConfiguration.DiagnosticsFilePath;
                if (path != null && File.Exists(path))
                {
                    var diagnosticsJson = File.ReadAllText(path);
                    configuration = JsonConvert.DeserializeObject<DiagnosticsServerConfiguration>(diagnosticsJson);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
