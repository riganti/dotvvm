using DotVVM.Framework.Compilation;
using Microsoft.Extensions.DependencyModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using IO=System.IO;

namespace DotVVM.Framework.Hosting
{
    record AssemblySerializableList(
        string[] assemblyDirs,
        string[] assemblyNames,
        string? mainAssembly
    )
    {
        public static AssemblySerializableList CreateFromCache(CompiledAssemblyCache cache)
        {
            [return: NotNullIfNotNull("a")]
            string? getName(Assembly? a)
            {
                if (a == null) return null;
                var n = a.GetName();
                return $"{n.Name}, Version={n.Version}";
            }
            var mainAssembly = Assembly.GetEntryAssembly();

            var assemblies = cache.GetReferencedAssemblies().ToList();
            if (mainAssembly is {})
                assemblies.Add(mainAssembly);
            return new AssemblySerializableList(
                assemblyDirs: assemblies.Select(a => IO.Path.GetDirectoryName(a.Location)).OfType<string>().Distinct().OrderBy(x => x).ToArray(),
                assemblyNames: assemblies.Select(getName).OrderBy(x => x).ToArray(),
                mainAssembly: getName(mainAssembly)
            );
        }
    }
}
