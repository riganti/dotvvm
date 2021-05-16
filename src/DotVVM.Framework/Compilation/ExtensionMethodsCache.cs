using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    public class ExtensionMethodsCache
    {
        private readonly CompiledAssemblyCache assemblyCache;
        private readonly ConcurrentDictionary<string, ImmutableArray<MethodInfo>> methodsCache;

        public ExtensionMethodsCache(CompiledAssemblyCache assemblyCache)
        {
            this.assemblyCache = assemblyCache;
            this.methodsCache = new ConcurrentDictionary<string, ImmutableArray<MethodInfo>>();
        }

        public IEnumerable<MethodInfo> GetExtensionsForNamespace(string @namespace)
        {
            return methodsCache.GetOrAdd(@namespace, (ns) => CreateExtensionsForNamespace(ns));
        }

        private ImmutableArray<MethodInfo> CreateExtensionsForNamespace(string @namespace)
        {
            var extensions = new List<MethodInfo>();

            foreach (var assembly in assemblyCache.GetAllAssemblies())
                foreach (var type in assembly.GetLoadableTypes().Where(t => t.Namespace == @namespace && t.IsClass && t.IsAbstract && t.IsSealed))
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.GetCustomAttribute(typeof(ExtensionAttribute)) != null))
                        extensions.Add(method);

            return extensions.ToImmutableArray();
        }
    }
}
