using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using System.Reflection;
using Newtonsoft.Json;

namespace DotVVM.Framework.ResourceManagement
{
    public abstract class LocalResourceLocation : ILocalResourceLocation
    {
        public string GetUrl(IDotvvmRequestContext context, string name) =>
            context.Configuration.ServiceLocator.GetService<ILocalResourceUrlManager>().GetResourceUrl(this, context, name);

        public abstract Stream LoadResource(IDotvvmRequestContext context);
    }

    public class LocalFileResourceLocation: LocalResourceLocation, IDebugFileLocalLocation
    {
        public string FilePath { get; }
        public LocalFileResourceLocation(string filePath)
        {
            if (filePath.StartsWith("~/", StringComparison.Ordinal)) filePath = filePath.Substring(2); // trim ~/ from the path
            this.FilePath = filePath;
        }

        public override Stream LoadResource(IDotvvmRequestContext context) => 
            File.OpenRead(Path.Combine(context.Configuration.ApplicationPhysicalPath, FilePath));

        public string GetFilePath(IDotvvmRequestContext context) => FilePath;
    }

    public class EmbededResourceLocation: LocalResourceLocation, IDebugFileLocalLocation
    {
        [JsonConverter(typeof(ReflectionAssemblyJsonConverter))]
        public Assembly Assembly { get; }
        public string Name { get; }
        /// <summary>
        /// File where the resource is located for debug purposes
        /// </summary>
        public string DebugFilePath { get; set; }
        public EmbededResourceLocation(Assembly assembly, string name, string debugFileName = null)
        {
            this.Name = name;
            this.Assembly = assembly;
            this.DebugFilePath = debugFileName;
        }

        public override Stream LoadResource(IDotvvmRequestContext context) =>
            context.Configuration.Debug && DebugFilePath != null && File.Exists(DebugFilePath) ?
            File.OpenRead(Path.Combine(context.Configuration.ApplicationPhysicalPath, DebugFilePath)) :
            Assembly.GetManifestResourceStream(Name);

        public string GetFilePath(IDotvvmRequestContext context) => DebugFilePath;
    }
}
