using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Diagnostics
{
    public class DiagnosticsServerConfiguration
    {
        private DateTime configurationLastWriteTimeUtc;

        public static string? DiagnosticsFilePath => Environment.GetEnvironmentVariable("TEMP") is string tmpPath
                ? Path.Combine(tmpPath, "DotVVM/diagnosticsConfiguration.json")
                : null;


        private Configuration config = new();

        public string? HostName => config.HostName;
        public int Port => config.Port;

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
                    var settings = DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe;
                    this.config = JsonSerializer.Deserialize<Configuration>(diagnosticsJson, settings).NotNull();
                }
            }
            catch
            {
                // ignored
            }
        }

        class Configuration
        {
            public string? HostName { get; set; }
            public int Port { get; set; }
        }
    }
}
