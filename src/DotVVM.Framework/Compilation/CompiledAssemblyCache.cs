using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;

#if DotNetCore
using Microsoft.Extensions.DependencyModel;
#endif

namespace DotVVM.Framework.Compilation
{
    public class CompiledAssemblyCache
    {

        private readonly ConcurrentDictionary<Assembly, MetadataReference> cachedAssemblyMetadata = new ConcurrentDictionary<Assembly, MetadataReference>();
        private readonly ConcurrentDictionary<string, Assembly> cachedAssemblies = new ConcurrentDictionary<string, Assembly>();

        private readonly DotvvmConfiguration configuration;

        public static CompiledAssemblyCache? Instance { get; private set; }

        public CompiledAssemblyCache(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;

#if DotNetCore
            System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += DefaultOnResolving;
#else
            AppDomain.CurrentDomain.AssemblyResolve += TryResolveAssembly;
#endif

            foreach (var assembly in BuildReferencedAssembliesCache(configuration))
            {
                cachedAssemblies.GetOrAdd(assembly.FullName.NotNull(), a => assembly);
            }

            if (Instance == null)
            {
                // normally, the constructor is not called multiple times as this service is singleton; only in unit tests, we create it multiple times
                Instance = this;
            }

            cache_AllNamespaces = new Lazy<HashSet<string>>(GetAllNamespaces);
        }

        internal static IEnumerable<Assembly> BuildReferencedAssembliesCache(DotvvmConfiguration configuration)
        {
            var diAssembly = typeof(ServiceCollection).Assembly;
            var markupAssemblies =
                configuration.Markup.Controls.Select(c => c.Assembly)
                    .Concat(configuration.Markup.Assemblies)
                    .Distinct()
                    .Where(s=> !string.IsNullOrWhiteSpace(s))
                    .Select(n => Assembly.Load(new AssemblyName(n)));

            var references = diAssembly.GetReferencedAssemblies().Select(Assembly.Load)
                .Concat(markupAssemblies)
                .Concat(new[] {
                    diAssembly,
                    Assembly.Load(new AssemblyName("mscorlib")),
                    Assembly.Load(new AssemblyName("System.ValueTuple")),
                    typeof(IServiceProvider).Assembly,
                    typeof(RuntimeBinderException).Assembly,
                    typeof(DynamicAttribute).Assembly,
                    typeof(DotvvmConfiguration).Assembly,
#if DotNetCore
                    Assembly.Load(new AssemblyName("System.Runtime")),
                    Assembly.Load(new AssemblyName("System.Private.CoreLib")),
                    Assembly.Load(new AssemblyName("System.Collections.Concurrent")),
                    Assembly.Load(new AssemblyName("System.Collections")),
#else
                    typeof(List<>).Assembly,
                    typeof(System.Net.WebUtility).Assembly
#endif
                });


            // Once netstandard assembly is loaded you cannot load it again! 
            var netstandardAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "netstandard");
            if (netstandardAssembly != null)
            {
                references = references.Concat(new[] { netstandardAssembly });
            }
            else
            {
                try
                {
                    // netstandard assembly is required for netstandard 2.0 and in some cases
                    // for netframework461 and newer. netstandard is not included in netframework452
                    // and will throw FileNotFoundException. Instead of detecting current netframework
                    // version, the exception is swallowed.
                    references = references.Concat(new[] { Assembly.Load(new AssemblyName("netstandard")) });
                }
                catch (FileNotFoundException) { }
            }

            return references.Distinct().ToList();
        }


#if DotNetCore
        /// <summary>
        /// Tries to resolve compiled assembly.
        /// </summary>
        private Assembly? DefaultOnResolving(System.Runtime.Loader.AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
        {
            return cachedAssemblies.TryGetValue(assemblyName.FullName, out var assembly) ? assembly : null;
        }
#else
        /// <summary>
        /// Tries to resolve compiled assembly.
        /// </summary>
        public Assembly? TryResolveAssembly(object sender, ResolveEventArgs args)
        {
            return cachedAssemblies.TryGetValue(args.Name, out var assembly) ? assembly : null;
        }
#endif

        public Assembly[] GetReferencedAssemblies()
        {
            return cachedAssemblies.Values.ToArray();
        }

        public Assembly[] GetAllAssemblies()
        {
            if (configuration.ExperimentalFeatures.ExplicitAssemblyLoading.Enabled)
            {
                return GetReferencedAssemblies();
            }
            else
            {
#if DotNetCore
                // auto-loads all referenced assemblies recursively
                return DependencyContext.Default.GetDefaultAssemblyNames().Select(Assembly.Load).ToArray();
#else
                // this doesn't load new assemblies, but it is done in InvokeStaticConstructorsOnAllControls
                return AppDomain.CurrentDomain.GetAssemblies();
#endif
            }
        }

        private readonly Lazy<HashSet<string>> cache_AllNamespaces;

        public bool IsAssemblyNamespace(string fullName) => cache_AllNamespaces.Value.Contains(fullName);

        private HashSet<string> GetAllNamespaces()
            => new HashSet<string>(GetAllAssemblies()
                .SelectMany(a => a.GetLoadableTypes()
                    .Select(t => t.Namespace!)
                    .Where(ns => ns is object))
                .Distinct(), StringComparer.Ordinal);


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
        internal void AddAssemblyMetadata(Assembly assembly, CompilationReference compilationReference)
        {
            cachedAssemblyMetadata[assembly] = compilationReference;
        }



        private ConcurrentDictionary<string, Type?> cache_FindTypeHash = new ConcurrentDictionary<string, Type?>(StringComparer.Ordinal);
        private ConcurrentDictionary<string, Type?> cache_FindTypeHashIgnoreCase = new ConcurrentDictionary<string, Type?>(StringComparer.OrdinalIgnoreCase);

        public Type? FindType(string name, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                return cache_FindTypeHashIgnoreCase.GetOrAdd(name, a => FindTypeCore(a, true));
            }
            return cache_FindTypeHash.GetOrAdd(name, a => FindTypeCore(a, false));
        }

        private Type? FindTypeCore(string name, bool ignoreCase)
        {
            var stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            // Type.GetType might sometimes work well
            var type = Type.GetType(name, false, ignoreCase);
            if (type != null) return type;

            var split = name.Split(',');
            name = split[0];

            var assemblies = GetAllAssemblies();
            if (split.Length > 1)
            {
                var assembly = split[1];
                return assemblies.Where(a => a.GetName().Name == assembly).Select(a => a.GetType(name))
                    .FirstOrDefault(t => t != null);
            }

            type = assemblies.Where(a => a.GetName().Name is string assemblyName && name.StartsWith(assemblyName, stringComparison))
                .Select(a => a.GetType(name, false, ignoreCase)).FirstOrDefault(t => t != null);
            if (type != null) return type;
            return assemblies.Select(a => a.GetType(name, false, ignoreCase)).FirstOrDefault(t => t != null);
        }

    }
}
