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

        public Func<IViewCompiler> ViewCompilerFactory { get; private set; }

        private ConcurrentDictionary<MarkupFile, (ControlBuilderDescriptor, Lazy<IControlBuilder>)> controlBuilders = new ConcurrentDictionary<MarkupFile, (ControlBuilderDescriptor, Lazy<IControlBuilder>)>();


        public DefaultControlBuilderFactory(DotvvmConfiguration configuration, IMarkupFileLoader markupFileLoader)
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

                var namespaceName = NamingHelper.GetNamespaceFromFileName(file.FileName, file.LastWriteDateTimeUtc, "DotvvmGeneratedViews");
                var assemblyName = namespaceName;
                var className = NamingHelper.GetClassFromFileName(file.FileName) + "ControlBuilder";
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
                    var (descriptor, factory) = ViewCompilerFactory().CompileView(file.ContentsReaderFactory(), file, assemblyName, namespaceName, className);
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
            var assemblies = ReflectionUtils.GetAllAssemblies().ToList();
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
            foreach (var assembly in assemblies)
            {
                // get already loaded assembly
                if (assembly.GetName().Name == cleanName)
                {
                    var codeBase = assembly.GetCodeBasePath();
                    if (codeBase!.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) return assembly;
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
                RegisterControlBuilder(builder.attribute.FilePath, (IControlBuilder)Activator.CreateInstance(builder.type).NotNull());
            }
        }

        public void RegisterControlBuilder(string file, IControlBuilder builder)
        {
            var markup = markupFileLoader.GetMarkup(configuration, file) ??
                         throw new Exception($"Could not load markup file {file}.");
            controlBuilders.TryAdd(markup, (new ControlBuilderDescriptor(builder.DataContextType, builder.ControlType), new Lazy<IControlBuilder>(() => builder)));
        }
    }
}
