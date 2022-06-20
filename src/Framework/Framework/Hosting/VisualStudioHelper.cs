using System.Diagnostics;
using System.IO;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotVVM.Framework.Hosting
{
    public static class VisualStudioHelper
    {
        internal static string SerializeConfig(DotvvmConfiguration config, bool includeProperties = true)
        {
            if (includeProperties)
            {
                // NB: Forces all properties to be registered
                config.ServiceProvider.GetRequiredService<IControlResolver>();
            }

            var obj = new {
                dotvvmVersion = typeof(DotvvmConfiguration).Assembly.GetName().Version.ToString(4),
                config,
                properties = includeProperties ? DotvvmPropertySerializableList.Properties : null,
                capabilities = includeProperties ? DotvvmPropertySerializableList.Capabilities : null,
                propertyGroups = includeProperties ? DotvvmPropertySerializableList.PropertyGroups : null,
            };
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                // suppress any errors that occur during serialization
                Error = (sender, args) => {
                    args.ErrorContext.Handled = true;
                },
                Converters = {
                    new StringEnumConverter(),
                    new ReflectionTypeJsonConverter(),
                    new ReflectionAssemblyJsonConverter()
                },
                ContractResolver = new DotvvmConfigurationSerializationResolver()
            });
        }

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
