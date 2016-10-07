using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace DotVVM.Framework.Compilation
{
    public class CompiledAssemblyCache
    {

        private readonly ConcurrentDictionary<Assembly, MetadataReference> cachedAssemblyMetadata = new ConcurrentDictionary<Assembly, MetadataReference>();
        private readonly ConcurrentDictionary<string, Assembly> cachedAssemblies = new ConcurrentDictionary<string, Assembly>();

#if DotNetCore
        /// <summary>
        /// Tries to resolve compiled assembly.
        /// </summary>
        private Assembly DefaultOnResolving(System.Runtime.Loader.AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
        {
            Assembly assembly;
            return cachedAssemblies.TryGetValue(assemblyName.FullName, out assembly) ? assembly : null;
        }
#else
        /// <summary>
        /// Tries to resolve compiled assembly.
        /// </summary>
        public Assembly TryResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly assembly;
            return cachedAssemblies.TryGetValue(args.Name, out assembly) ? assembly : null;
        }
#endif


        /// <summary>
        /// Gets the <see cref="MetadataReference"/> for the specified assembly.
        /// </summary>
        public MetadataReference GetAssemblyMetadata(Assembly assembly)
        {
            return cachedAssemblyMetadata.GetOrAdd(assembly, a => MetadataReference.CreateFromFile(a.Location));
        }

        /// <summary>
        /// Adds the assembly to the cache.
        /// </summary>
        internal void AddAssembly(Assembly assembly, CompilationReference compilationReference)
        {
            cachedAssemblyMetadata[assembly] = compilationReference;
            cachedAssemblies[assembly.FullName] = assembly;
        }



        /// <summary>
        /// Gets the singleton instance of the assembly cache.
        /// </summary>
        public static CompiledAssemblyCache Instance { get; private set; }

        static CompiledAssemblyCache()
        {
            Instance = new CompiledAssemblyCache();

#if DotNetCore
            System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += Instance.DefaultOnResolving;
#else
            AppDomain.CurrentDomain.AssemblyResolve += Instance.TryResolveAssembly;
#endif
        }

    }
}
