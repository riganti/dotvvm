using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotVVM.Framework.Configuration;
using Newtonsoft.Json;

namespace DotVVM.Framework.Diagnostics
{

    public class DotvvmDiagnosticsConfiguration
    {
        private DiagnosticsServerConfiguration configuration;
        private DateTime configurationLastWriteTimeUtc;

        public string GetDiagnosticsServerHostname()
        {
            RefreshConfiguration();
            return configuration?.HostName;
        }

        public int? GetDiagnosticsServerPort()
        {
            RefreshConfiguration();
            return configuration?.Port;
        }

        private void RefreshConfiguration()
        {
            try
            {
                var path = DiagnosticsServerConfiguration.DiagnosticsFilePath;
                var info = new FileInfo(path);
                if (info.Exists && configurationLastWriteTimeUtc != info.LastWriteTimeUtc)
                {
                    configurationLastWriteTimeUtc = info.LastWriteTimeUtc;

                    var diagnosticsJson = File.ReadAllText(path);
                    var settings = DefaultSerializerSettingsProvider.Instance.Settings;
                    configuration = JsonConvert.DeserializeObject<DiagnosticsServerConfiguration>(diagnosticsJson, settings);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
