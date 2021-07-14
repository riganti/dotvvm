#nullable enable
using System.IO;
using DotVVM.Framework.Hosting;
using System.Reflection;
using Newtonsoft.Json;
using System;
using DotVVM.Framework.Utils;

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
        public string DebugName { get; }
        /// <summary>
        /// File where the resource is located for debug purposes
        /// </summary>
        public string? DebugFilePath { get; set; }
        public EmbeddedResourceLocation(Assembly assembly, string name, string? debugFilePath = null, string? debugName = null)
        {
            if (assembly.GetManifestResourceInfo(name) == null) throw new ArgumentException($"Could not find resource '{name}' in assembly {assembly.GetName().Name}. Did you mean one of {string.Join(", ", assembly.GetManifestResourceNames())}?", nameof(name));

            this.Name = name;
            this.Assembly = assembly;

            if (debugName != null)
            {
                if (assembly.GetManifestResourceInfo(debugName) == null) throw new ArgumentException($"Could not find resource '{debugName}' in assembly {assembly.GetName().Name}. Did you mean one of {string.Join(", ", assembly.GetManifestResourceNames())}?", nameof(debugName));
                this.DebugName = debugName;
            }
            else
            {
                this.DebugName = name;
            }

            this.DebugFilePath = debugFilePath;
        }

        public override Stream LoadResource(IDotvvmRequestContext context)
        {
            var debugPath = DebugFilePath == null ? null : Path.Combine(context.Configuration.ApplicationPhysicalPath, DebugFilePath);
            return context.Configuration.Debug && debugPath != null && File.Exists(debugPath) ?
                File.OpenRead(debugPath) :
                Assembly.GetManifestResourceStream(context.Configuration.Debug ? DebugName : Name).NotNull();
        }

        public string? GetFilePath(IDotvvmRequestContext context) => DebugFilePath;
    }
}
