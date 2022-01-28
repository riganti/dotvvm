using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotVVM.Framework.Configuration;
using Newtonsoft.Json;

namespace DotVVM.Framework.Diagnostics
{
    public class DiagnosticsServerConfiguration
    {
        private DateTime configurationLastWriteTimeUtc;

        [JsonIgnore]
        public static string? DiagnosticsFilePath => Environment.GetEnvironmentVariable("TEMP") is string tmpPath
                ? Path.Combine(tmpPath, "DotVVM/diagnosticsConfiguration.json")
                : null;

        public string? HostName { get; set; }
        public int Port { get; set; }

        public string? GetFreshHostName()
        {
            RefreshConfiguration();
            return HostName;
        }

        public int? GetFreshPort()
        {
            RefreshConfiguration();
            return Port;
        }

        private void RefreshConfiguration()
        {
            try
            {
                var path = DiagnosticsFilePath;
                if (path is null)
                    return;
                var info = new FileInfo(path);
                if (info.Exists && configurationLastWriteTimeUtc != info.LastWriteTimeUtc)
                {
                    configurationLastWriteTimeUtc = info.LastWriteTimeUtc;

                    var diagnosticsJson = File.ReadAllText(path);
                    var settings = DefaultSerializerSettingsProvider.Instance.Settings;
                    JsonConvert.PopulateObject(diagnosticsJson, this, settings);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
