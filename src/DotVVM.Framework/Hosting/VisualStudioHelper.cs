using System.Diagnostics;
using System.IO;
using DotVVM.Framework.Configuration;
using Newtonsoft.Json;

namespace DotVVM.Framework.Hosting
{
    public static class VisualStudioHelper
    {
        public static void DumpConfiguration(DotvvmConfiguration config, string directory)
        {
            if (Process.GetCurrentProcess().ProcessName == "iisexpress")
            {
                try
                {
                    File.WriteAllText(Path.Combine(directory, "dotvvm_serialized_config.json.tmp"), JsonConvert.SerializeObject(config));
                }
                catch
                {
                }
            }
        }
    }
}
