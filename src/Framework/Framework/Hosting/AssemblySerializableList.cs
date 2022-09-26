using DotVVM.Framework.Compilation;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Reflection;
using IO=System.IO;

namespace DotVVM.Framework.Hosting
{
    record AssemblySerializableList(
        string[] assemblyDirs,
        string[] assemblyNames,
        string mainAssembly
    )
    {
        public static AssemblySerializableList CreateFromCache(CompiledAssemblyCache cache)
        {
            string getName(Assembly a)
            {
                var n = a.GetName();
                return $"{n.Name}, Version={n.Version}";
            }
            var mainAssembly = Assembly.GetEntryAssembly();

            var assemblies = cache.GetReferencedAssemblies();
            return new AssemblySerializableList(
                assemblyDirs: assemblies.Append(mainAssembly).Select(a => IO.Path.GetDirectoryName(a.Location)).Distinct().OrderBy(x => x).ToArray(),
                assemblyNames: assemblies.Select(getName).OrderBy(x => x).ToArray(),
                mainAssembly: getName(mainAssembly)
            );
        }
    }
}
