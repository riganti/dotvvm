#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation
{
    /// <summary>
    /// Provides control builder objects for markup files.
    /// </summary>
    public class DefaultControlBuilderFactory : IControlBuilderFactory
    {
        private readonly DotvvmConfiguration configuration;
        private readonly IMarkupFileLoader markupFileLoader;
        private readonly CompiledAssemblyCache compiledAssemblyCache;

        public Func<IViewCompiler> ViewCompilerFactory { get; private set; }

        private ConcurrentDictionary<MarkupFile, (ControlBuilderDescriptor, Lazy<IControlBuilder>)> controlBuilders = new ConcurrentDictionary<MarkupFile, (ControlBuilderDescriptor, Lazy<IControlBuilder>)>();


        public DefaultControlBuilderFactory(DotvvmConfiguration configuration, IMarkupFileLoader markupFileLoader, CompiledAssemblyCache compiledAssemblyCache)
        {
            for (int i = 0; i < compilationLocks.Length; i++)
            {
                compilationLocks[i] = new object();
            }

            this.configuration = configuration;

            // WORKAROUND: there is a circular dependency
            // TODO: get rid of that
            this.ViewCompilerFactory = () => configuration.ServiceProvider.GetRequiredService<IViewCompiler>();
            this.markupFileLoader = markupFileLoader;
            this.compiledAssemblyCache = compiledAssemblyCache;
            if (configuration.CompiledViewsAssemblies != null)
                foreach (var assembly in configuration.CompiledViewsAssemblies)
                {
                    LoadCompiledViewsAssembly(assembly);
                }
        }


        /// <summary>
        /// Gets the control builder.
        /// </summary>
        public (ControlBuilderDescriptor descriptor, Lazy<IControlBuilder> builder) GetControlBuilder(string virtualPath)
        {
            var markupFile = markupFileLoader.GetMarkup(configuration, virtualPath) ?? throw new DotvvmCompilationException($"File '{virtualPath}' was not found. This exception is possibly caused because of incorrect route registration.");
            return controlBuilders.GetOrAdd(markupFile, CreateControlBuilder);
        }

        object[] compilationLocks = new object[Environment.ProcessorCount * 2];

        /// <summary>
        /// Creates the control builder.
        /// </summary>
        private (ControlBuilderDescriptor, Lazy<IControlBuilder>) CreateControlBuilder(MarkupFile file)
        {
            var lockId = (file.GetHashCode() & 0x7fffffff) % compilationLocks.Length;
            // do not compile the same view multiple times
            lock (compilationLocks[lockId])
            {
                if (controlBuilders.ContainsKey(file)) return controlBuilders[file];

                var namespaceName = GetNamespaceFromFileName(file.FileName, file.LastWriteDateTimeUtc);
                var assemblyName = namespaceName;
                var className = GetClassFromFileName(file.FileName) + "ControlBuilder";
                void editCompilationException(DotvvmCompilationException ex)
                {
                    if (ex.FileName == null)
                        ex.FileName = file.FullPath;
                    else if (!Path.IsPathRooted(ex.FileName))
                        ex.FileName = Path.Combine(
                            file.FullPath.Remove(file.FullPath.Length - file.FileName.Length),
                            ex.FileName);
                }
                try
                {
                    var (descriptor, factory) = ViewCompilerFactory().CompileView(file.ContentsReaderFactory(), file.FileName, assemblyName, namespaceName, className);

                    if (descriptor.ViewModuleReferences != null)
                    {
                        foreach (var resource in descriptor.ViewModuleReferences)
                        {
                            var (import, init) = resource.BuildResources(configuration.Resources);
                            configuration.Resources.RegisterViewModuleResources(import, init);
                        }
                    }

                    return (descriptor, new Lazy<IControlBuilder>(() => {
                        try { return factory(); }
                        catch (DotvvmCompilationException ex)
                        {
                            editCompilationException(ex);
                            throw;
                        }
                    }));
                }
                catch (DotvvmCompilationException ex)
                {
                    editCompilationException(ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the name of the class from the file name.
        /// </summary>
        public static string GetClassFromFileName(string fileName)
        {
            return GetValidIdentifier(Path.GetFileNameWithoutExtension(fileName));
        }

        protected static string GetValidIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) return "_";
            var arr = identifier.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                if (!char.IsLetterOrDigit(arr[i]))
                {
                    arr[i] = '_';
                }
            }
            identifier = new string(arr);
            if (char.IsDigit(arr[0])) identifier = "C" + identifier;
            if (csharpKeywords.Contains(identifier)) identifier += "0";
            return identifier;
        }

        /// <summary>
        /// Gets the name of the namespace from the file name.
        /// </summary>
        public static string GetNamespaceFromFileName(string fileName, DateTime lastWriteDateTimeUtc)
        {
            // TODO: make sure crazy directory names are ok, it should also work on linux :)

            // replace \ and / for .
            var parts = fileName.Split(new[] { '/', '\\' });
            parts[parts.Length - 1] = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);

            fileName = string.Join(".", parts.Select(GetValidIdentifier));
            return "DotvvmGeneratedViews" + fileName + "_" + lastWriteDateTimeUtc.Ticks;
        }

        public void LoadCompiledViewsAssembly(string filePath)
        {
            var assembly = TryFindAssembly(filePath);
            if (assembly != null)
            {
                LoadCompiledViewsAssembly(assembly);

                var bindings = Path.Combine(Path.GetDirectoryName(assembly.GetCodeBasePath())!, "CompiledViewsBindings.dll");
                if (File.Exists(bindings)) AssemblyLoader.LoadFile(bindings);
            }
        }

        public Assembly? TryFindAssembly(string fileName)
        {
            if (File.Exists(fileName)) return AssemblyLoader.LoadFile(fileName);
            if (Path.IsPathRooted(fileName)) return null;
            var cleanName = Path.GetFileNameWithoutExtension(Path.GetFileName(fileName));
            var assemblies = compiledAssemblyCache.GetAllAssemblies();
            foreach (var assembly in assemblies)
            {
                // get already loaded assembly
                if (assembly.GetName().Name == cleanName)
                {
                    var codeBase = assembly.GetCodeBasePath();
                    if (codeBase!.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) return assembly;
                }
            }
            foreach (var assemblyDirectory in new[] { Path.GetDirectoryName(typeof(DefaultControlBuilderFactory).GetTypeInfo().Assembly.GetCodeBasePath()), configuration.ApplicationPhysicalPath })
            {
                if (!string.IsNullOrEmpty(assemblyDirectory))
                {
                    var possibleFileName = Path.Combine(assemblyDirectory, fileName);
                    if (File.Exists(possibleFileName)) return AssemblyLoader.LoadFile(possibleFileName);
                }
            }
            return null;
        }

        public void LoadCompiledViewsAssembly(Assembly assembly)
        {
            var initMethods = assembly.GetTypes()
                .Where(t => t.Name == "SerializedObjects")
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(m => m.Name == "Init")
                .ToArray();
            foreach (var initMethod in initMethods)
            {
                var args = initMethod.GetParameters().Select(p => configuration.ServiceProvider.GetRequiredService(p.ParameterType)).ToArray();
                initMethod.Invoke(null, args);
            }
            var builders = assembly.GetTypes().Select(t => new {
                type = t,
                attribute = t.GetTypeInfo().GetCustomAttribute<LoadControlBuilderAttribute>()
            }).Where(t => t.attribute != null);
            foreach (var builder in builders)
            {
                RegisterControlBuilder(builder.attribute!.FilePath, (IControlBuilder)Activator.CreateInstance(builder.type).NotNull());
            }
        }

        public void RegisterControlBuilder(string file, IControlBuilder builder)
        {
            var markup = markupFileLoader.GetMarkup(configuration, file) ??
                         throw new Exception($"Could not load markup file {file}.");
            controlBuilders.TryAdd(markup, (builder.Descriptor, new Lazy<IControlBuilder>(() => builder)));
        }

        private static readonly HashSet<string> csharpKeywords = new HashSet<string>(new[]
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
            "using", "virtual", "void", "volatile", "while", "add", "alias", "ascending", "async", "await", "descending", "dynamic", "from", "get", "global", "group", "into",
            "join", "let", "orderby", "partial", "remove", "select", "set", "value", "var", "where", "where", "yield"
        });
    }
}
