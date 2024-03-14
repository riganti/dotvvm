using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;

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

            var dotvvmVersion = (typeof(DotvvmConfiguration).Assembly.GetName().Version ?? new System.Version(0, 0, 0, 0));
            var obj = new {
                dotvvmVersion = dotvvmVersion.ToString(4),
                config,
                properties = includeProperties ? DotvvmPropertySerializableList.Properties : null,
                capabilities = includeProperties ? DotvvmPropertySerializableList.Capabilities : null,
                propertyGroups = includeProperties ? DotvvmPropertySerializableList.PropertyGroups : null,
                controls = includeProperties ? DotvvmPropertySerializableList.GetControls(config.ServiceProvider.GetRequiredService<CompiledAssemblyCache>()) : null,
                assemblies = includeProperties ? AssemblySerializableList.CreateFromCache(config.ServiceProvider.GetRequiredService<CompiledAssemblyCache>()) : null,
            };
            return JsonSerializer.Serialize(obj, GetSerializerOptions());
        }

        public static JsonSerializerOptions GetSerializerOptions()
        {
            return new JsonSerializerOptions {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                // ReferenceHandler = ReferenceHandler.IgnoreCycles, // doesn't work together with JsonObjectCreationHandling.Populate
                // Error = (sender, args) => { // TODO: how? https://github.com/dotnet/runtime/issues/38049
                //     args.ErrorContext.Handled = true;
                // },
                WriteIndented = true,
                Converters = {
                    new ReflectionTypeJsonConverter(),
                    new ReflectionAssemblyJsonConverter(),
                    new DotvvmTypeDescriptorJsonConverter<ITypeDescriptor>(),
                    new DotvvmPropertyJsonConverter(),
                    new DotvvmEnumConverter(),
                    new DataContextChangeAttributeConverter(),
                    new DataContextManipulationAttributeConverter()
                },
                TypeInfoResolver = new DotvvmConfigurationSerializationResolver(),
                Encoder = DefaultSerializerSettingsProvider.Instance.HtmlSafeLessParaoidEncoder
            };
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
