using System.Diagnostics;
using System.IO;
using DotVVM.Framework.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotVVM.Framework.Hosting
{
    public static class VisualStudioHelper
    {
        internal static string SerializeConfig(DotvvmConfiguration config) =>
            JsonConvert.SerializeObject(config, Formatting.Indented, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = {
                    new StringEnumConverter()
                },
                ContractResolver = new DotvvmConfigurationSerializationResolver()
            });

        public static void DumpConfiguration(DotvvmConfiguration config, string directory)
        {
            if (config.Debug || Debugger.IsAttached || Process.GetCurrentProcess().ProcessName == "iisexpress")
            {
                try
                {
                    File.WriteAllText(Path.Combine(directory, "dotvvm_serialized_config.json.tmp"), SerializeConfig(config));
                }
                catch
                {
                }
            }
        }
    }
}
