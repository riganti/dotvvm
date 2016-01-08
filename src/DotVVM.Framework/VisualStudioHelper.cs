using DotVVM.Framework.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework
{
    public static class VisualStudioHelper
    {
        public static void SaveConfiguration(DotvvmConfiguration config, string directory)
        {
            if (Process.GetCurrentProcess().ProcessName == "iisexpress") {
                try {
                    File.WriteAllText(Path.Combine(directory, "dotvvm_serialized_config.json.tmp"), JsonConvert.SerializeObject(config));
                }
                catch {
                }
            }
        }
    }
}
