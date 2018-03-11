using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DotVVM.Framework.Diagnostics
{

    public class DotvvmDiagnosticsConfiguration
    {
       

        private DiagnosticsServerConfiguration configuration;
        private DateTime configurationLastWriteTimeUtc;

        public string DiagnosticsServerHostname
        {
            get
            {
                EnsureConfigurationValid();
                return configuration?.HostName;
            }
        }



        public int? DiagnosticsServerPort
        {
            get
            {
                EnsureConfigurationValid();
                return configuration?.Port;
            }
        }

        private void EnsureConfigurationValid()
        {
            try
            {
                var path = DiagnosticsServerConfiguration.DiagnosticsFilePath;
                var info = new FileInfo(path);
                if (info.Exists && configurationLastWriteTimeUtc != info.LastWriteTimeUtc)
                {
                    configurationLastWriteTimeUtc = info.LastWriteTimeUtc;

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
