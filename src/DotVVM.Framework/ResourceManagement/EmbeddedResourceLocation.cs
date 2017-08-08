using System.IO;
using DotVVM.Framework.Hosting;
using System.Reflection;
using Newtonsoft.Json;
using System;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Represents resource located in assembly embedded resources.
    /// </summary>
    public class EmbeddedResourceLocation : LocalResourceLocation, IDebugFileLocalLocation
    {
        [JsonConverter(typeof(ReflectionAssemblyJsonConverter))]
        public Assembly Assembly { get; }
        public string Name { get; }
        /// <summary>
        /// File where the resource is located for debug purposes
        /// </summary>
        public string DebugFilePath { get; set; }
        public EmbeddedResourceLocation(Assembly assembly, string name, string debugFileName = null)
        {
            if (assembly.GetManifestResourceInfo(name) == null) throw new ArgumentException($"Could not find resource '{name}' in assembly {assembly.GetName().Name}", nameof(name));

            this.Name = name;
            this.Assembly = assembly;
            this.DebugFilePath = debugFileName;
        }

        public override Stream LoadResource(IDotvvmRequestContext context)
        {
            var debugPath = DebugFilePath == null ? null : Path.Combine(context.Configuration.ApplicationPhysicalPath, DebugFilePath);
            return context.Configuration.Debug && debugPath != null && File.Exists(debugPath) ?
                File.OpenRead(debugPath) :
                Assembly.GetManifestResourceStream(Name);
        }

        public string GetFilePath(IDotvvmRequestContext context) => DebugFilePath;
    }
}
