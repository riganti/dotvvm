using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DotVVM.Framework.Diagnostics
{
    public class DiagnosticsServerConfiguration
    {
        [JsonIgnore]
        public static string? DiagnosticsFilePath => Environment.GetEnvironmentVariable("TEMP") is string tmpPath ? Path.Combine(tmpPath, "DotVVM/diagnosticsConfiguration.json") : null;


        public string? HostName { get; set; }
        public int Port { get; set; }

    }
}
