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
        public static string DiagnosticsFilePath => Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "DotVVM/diagnosticsConfiguration.json");


        public string HostName { get; set; }
        public int Port { get; set; }
        
    }
}
