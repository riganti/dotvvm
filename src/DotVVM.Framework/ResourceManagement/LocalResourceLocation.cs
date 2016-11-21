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
        private string url;
        public string GetUrl(IDotvvmRequestContext context, string name) =>
            url ?? (url = context.Configuration.ServiceLocator.GetService<ILocalResourceUrlManager>().GetResourceUrl(this, context, name));

        public abstract Stream LoadResource(IDotvvmRequestContext context);
    }

    public class LocalFileResourceLocation: LocalResourceLocation
    {
        public string FilePath { get; }
        public LocalFileResourceLocation(string filePath)
        {
            if (filePath.StartsWith("~/", StringComparison.Ordinal)) filePath = filePath.Substring(2); // trim ~/ from the path
            this.FilePath = filePath;
        }

        public override Stream LoadResource(IDotvvmRequestContext context) => 
            File.OpenRead(Path.Combine(context.Configuration.ApplicationPhysicalPath, FilePath));
    }

    public class EmbededResourceLocation: LocalResourceLocation
    {
        [JsonConverter(typeof(ReflectionAssemblyJsonConverter))]
        public Assembly Assembly { get; }
        public string Name { get; }
        public EmbededResourceLocation(Assembly assembly, string name)
        {
            this.Name = name;
            this.Assembly = assembly;
        }

        public override Stream LoadResource(IDotvvmRequestContext context) =>
            Assembly.GetManifestResourceStream(Name);
    }
}
