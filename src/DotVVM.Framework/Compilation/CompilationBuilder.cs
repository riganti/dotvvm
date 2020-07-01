using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation
{
    public class CompilationBuilder
    {
        private readonly CompiledAssemblyCache assemblyCache;
        private readonly Lazy<IEnumerable<Assembly>> referencedAssembliesCache;
        private readonly DotvvmMarkupConfiguration markupConfiguration;
        private readonly string assemblyName;

        private CSharpCompilation compilation;
        private List<ResourceDescription> resources = new List<ResourceDescription>();

        public CompilationBuilder(DotvvmMarkupConfiguration markupConfiguration, string assemblyName)
        {
            this.markupConfiguration = markupConfiguration;
            this.assemblyName = assemblyName;

            this.assemblyCache = CompiledAssemblyCache.Instance;
            this.referencedAssembliesCache = new Lazy<IEnumerable<Assembly>>(BuildReferencedAssembliesCache, true);

            this.compilation = CSharpCompilation.Create(assemblyName, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(referencedAssembliesCache.Value.Select(a => assemblyCache.GetAssemblyMetadata(a)));
        }
        
        private IEnumerable<Assembly> BuildReferencedAssembliesCache()
        {
            var diAssembly = typeof(ServiceCollection).Assembly;

            var references = diAssembly.GetReferencedAssemblies().Select(Assembly.Load)
                .Concat(markupConfiguration.Assemblies.Select(e => Assembly.Load(new AssemblyName(e))))
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
                    Assembly.Load(new AssemblyName("System.Collections.Concurrent")),
                    Assembly.Load(new AssemblyName("System.Collections")),
                    typeof(object).Assembly
#else
                    typeof(List<>).Assembly
#endif
                });

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


        public void AddToCompilation(IEnumerable<SyntaxTree> trees, IEnumerable<UsedAssembly> assemblies)
        {
            compilation = compilation
                .AddSyntaxTrees(trees)
                .AddReferences(assemblies
                    .Select(a => assemblyCache.GetAssemblyMetadata(a.Assembly).WithAliases(ImmutableArray.Create(a.Identifier, "global"))));
        }

        /// <summary>
        /// Builds the assembly.
        /// </summary>
        public Assembly BuildAssembly()
        {
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms, manifestResources: resources);
                if (result.Success)
                {
                    var assembly = AssemblyLoader.LoadRaw(ms.ToArray());
                    assemblyCache.AddAssembly(assembly, compilation.ToMetadataReference());
                    return assembly;
                }
                else
                {
                    throw new DotvvmCompilationException("The compilation failed! This is most probably bug in the DotVVM framework.\r\n\r\n"
                                        + string.Join("\r\n", result.Diagnostics)
                                        + "\r\n\r\n" + compilation.SyntaxTrees[0].GetRoot().NormalizeWhitespace() + "\r\n\r\n"
                                        + "References: " + string.Join("\r\n", compilation.ReferencedAssemblyNames.Select(n => n.Name)));
                }
            }
        }

        public CSharpCompilation GetCompilation()
        {
            return compilation;
        }

        public void AddReferences(params MetadataReference[] references)
        {
            compilation = compilation.AddReferences(references);
        }
    }
}
