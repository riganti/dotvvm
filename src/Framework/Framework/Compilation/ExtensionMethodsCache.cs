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
using DotVVM.Framework.Runtime;
using System.Diagnostics;

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

            HotReloadMetadataUpdateHandler.ExtensionMethodsCaches.Add(new(this));
        }

        public IEnumerable<MethodInfo> GetExtensionsForNamespaces(string[] @namespaces)
        {
            var results = namespaces.Select(x => methodsCache.GetValueOrDefault(x)).ToArray();

            if (!results.Any(x => x.IsDefault))
                return results.SelectMany(x => x);


            // the lock is there just to prevent parallel scanning of the same namespaces which is quite common with parallel compilation mode.
            // it's most likely the same namespaces, so it won't help at all - only run into lock contention in System.Reflection
            lock (methodsCache)
            {
                results = namespaces.Select(x => methodsCache.GetValueOrDefault(x)).ToArray();
                var missingNamespaces = namespaces.Where(x => !methodsCache.ContainsKey(x)).ToArray();

                var createdNamespaces = CreateExtensionsForNamespaces(missingNamespaces);
                for (int i = 0; i < missingNamespaces.Length; i++)
                {
                    methodsCache.TryAdd(missingNamespaces[i], createdNamespaces[i]);
                }

                return namespaces.SelectMany(x => methodsCache.GetValue(x));
            }
        }

        private ImmutableArray<MethodInfo>[] CreateExtensionsForNamespaces(string[] namespaces)
        {
            if (namespaces.Length == 0)
                return Array.Empty<ImmutableArray<MethodInfo>>();
            // we fetch extension methods for multiple namespaces at once for performance reasons
            // this way we get rid of few of these "full scans"
            var results = namespaces.Select(x => ImmutableArray.CreateBuilder<MethodInfo>()).ToArray();


            foreach (var assembly in assemblyCache.GetAllAssemblies())
                foreach (var type in assembly.GetLoadableTypes())
                {
                    // check all flags first, these are much cheaper to get than the namespace
                    if (!(type.IsAbstract && type.IsSealed && type.IsClass))
                        continue;
                    var typeNamespace = type.Namespace;
                    for (int i = 0; i < namespaces.Length; i++)
                    {
                        if (!namespaces[i].Equals(typeNamespace, StringComparison.Ordinal)) continue;

                        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                        {
                            if (method.IsDefined(typeof(ExtensionAttribute)))
                                results[i].Add(method);
                        }

                        break;
                    }
                }

            return results.Select(x => x.ToImmutableArray()).ToArray();
        }

        /// <summary> Clear cache when hot reload happens </summary>
        internal void ClearCaches(Type[] types)
        {
            foreach (var t in types)
                methodsCache.TryRemove(t.Namespace ?? "", out _);
        }

    }
}
