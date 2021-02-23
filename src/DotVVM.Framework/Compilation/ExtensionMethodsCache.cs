using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotVVM.Framework.Compilation
{
    public class ExtensionMethodsCache
    {
        private readonly CompiledAssemblyCache assemblyCache;
        private readonly ConcurrentDictionary<string, List<MethodInfo>> methodsCache;

        public ExtensionMethodsCache(CompiledAssemblyCache assemblyCache)
        {
            this.assemblyCache = assemblyCache;
            this.methodsCache = new ConcurrentDictionary<string, List<MethodInfo>>();
        }

        public IEnumerable<MethodInfo> GetExtensionsForNamespace(string @namespace)
        {
            if (!methodsCache.TryGetValue(@namespace, out var extensions))
                return CreateExtensionsForNamespace(@namespace);

            return extensions;
        }

        private List<MethodInfo> CreateExtensionsForNamespace(string @namespace)
        {
            var extensions = new List<MethodInfo>();

            foreach (var assembly in assemblyCache.GetAllAssemblies())
                foreach (var type in assembly.GetTypes().Where(t => t.Namespace == @namespace && t.IsClass && t.IsAbstract && t.IsSealed))
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.GetCustomAttribute(typeof(ExtensionAttribute)) != null))
                        extensions.Add(method);

            methodsCache.TryAdd(@namespace, extensions);
            return extensions;
        }
    }
}
