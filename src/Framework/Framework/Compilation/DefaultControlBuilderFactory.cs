using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ViewCompiler;
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
        private readonly bool allowReload;
        private readonly IMarkupFileLoader markupFileLoader;
        private readonly CompiledAssemblyCache compiledAssemblyCache;

        public Func<IViewCompiler> ViewCompilerFactory { get; private set; }

        private ConcurrentDictionary<string, Lazy<(ControlBuilderDescriptor, Lazy<IControlBuilder>)>> controlBuilders = new();

        public DefaultControlBuilderFactory(DotvvmConfiguration configuration, IMarkupFileLoader markupFileLoader, CompiledAssemblyCache compiledAssemblyCache)
        {
            this.configuration = configuration;
            this.allowReload = configuration.Debug; // TODO: do we want another option for this?

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
            var (markupFile, markupChanged) = GetMarkupFile(virtualPath);
            if (!markupChanged)
            {
                if (controlBuilders.TryGetValue(virtualPath, out var builder))
                {
                    return builder.Value;
                }
                // because race conditions, this can happen, so just let if fallback onto creating the builder again
            }

            var lazy = new Lazy<(ControlBuilderDescriptor, Lazy<IControlBuilder>)>(() =>
                CreateControlBuilder(markupFile));

            if (allowReload)
            {
                return (controlBuilders[virtualPath] = lazy).Value;
            }
            else
            {
                return controlBuilders.GetOrAdd(virtualPath, lazy).Value;
            }
        }


        readonly ConcurrentDictionary<string, MarkupFile> markupFiles = new();
        private (MarkupFile file, bool changed) GetMarkupFile(string virtualPath)
        {
            if (markupFiles.TryGetValue(virtualPath, out var cachedFile))
            {
                if (!allowReload)
                    return (cachedFile, false);
                
                var newFile = markupFileLoader.GetMarkup(configuration, virtualPath);
                if (newFile is null || cachedFile.Equals(newFile))
                    return (cachedFile, false);
                else
                {
                    markupFiles[virtualPath] = newFile;
                    return (newFile, true);
                }
            }
            var markupFile = markupFileLoader.GetMarkup(configuration, virtualPath) ?? throw new DotvvmCompilationException($"File '{virtualPath}' was not found. This exception is possibly caused because of incorrect route registration.");
            markupFiles.TryAdd(virtualPath, markupFile);
            return (markupFile, true);
        }

        /// <summary>
        /// Creates the control builder.
        /// </summary>
        private (ControlBuilderDescriptor, Lazy<IControlBuilder>) CreateControlBuilder(MarkupFile file)
        {
            var compilationService = configuration.ServiceProvider.GetService<IDotvvmViewCompilationService>();
            void editCompilationException(DotvvmCompilationException ex)
            {
                if (ex.FileName == null)
                {
                    ex.FileName = file.FullPath;
                }
                else if (!Path.IsPathRooted(ex.FileName))
                {
                    ex.FileName = Path.Combine(
                        file.FullPath.Remove(file.FullPath.Length - file.FileName.Length),
                        ex.FileName);
                }
            }
            try
            {
                var sw = ValueStopwatch.StartNew();
                var (descriptor, factory) = ViewCompilerFactory().CompileView(file.ReadContent(), file.FileName);
                var phase1Ticks = sw.ElapsedTicks;

                var lazyBuilder = new Lazy<IControlBuilder>(() => {
                    try
                    {
                        sw.Restart();
                        var result = factory();

                        // register the internal resource after the page is successfully compiled,
                        // otherwise we could be hiding compile error behind more cryptic resource registration errors
                        if (descriptor.ViewModuleReference != null)
                        {
                            var (import, init) = descriptor.ViewModuleReference.BuildResources(configuration.Resources);
                            configuration.Resources.RegisterViewModuleResources(import, init);
                        }
                        Interlocked.Increment(ref DotvvmMetrics.BareCounters.ViewsCompiledOk);

                        compilationService.RegisterCompiledView(file.FileName, descriptor, null);
                        return result;
                    }
                    catch (DotvvmCompilationException ex)
                    {
                        Interlocked.Increment(ref DotvvmMetrics.BareCounters.ViewsCompiledFailed);
                        editCompilationException(ex);
                        compilationService.RegisterCompiledView(file.FileName, descriptor, ex);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        compilationService.RegisterCompiledView(file.FileName, descriptor, ex);
                        throw;
                    }
                    finally
                    {
                        Interlocked.Add(ref DotvvmMetrics.BareCounters.ViewsCompilationTime, phase1Ticks + sw.ElapsedTicks);
                    }
                });

                // initialize the Lazy asynchronously to speed up initialization and get reasonably accurate ViewsCompilationTime metric
                Task.Run(() => {
                    try {
                        _ = lazyBuilder.Value;
                    } catch { }
                });

                return (descriptor, lazyBuilder);
            }
            catch (DotvvmCompilationException ex)
            {
                editCompilationException(ex);
                compilationService.RegisterCompiledView(file.FileName, null, ex);
                throw;
            }
            catch (Exception ex)
            {
                compilationService.RegisterCompiledView(file.FileName, null, ex);
                throw;
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
            foreach (var assemblyDirectory in new[] { Path.GetDirectoryName(typeof(DefaultControlBuilderFactory).Assembly.GetCodeBasePath()), configuration.ApplicationPhysicalPath })
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
                attribute = t.GetCustomAttribute<LoadControlBuilderAttribute>()
            }).Where(t => t.attribute != null);
            foreach (var builder in builders)
            {
                RegisterControlBuilder(builder.attribute!.FilePath, (IControlBuilder)Activator.CreateInstance(builder.type).NotNull());
            }
        }

        public void RegisterControlBuilder(string file, IControlBuilder builder)
        {
            controlBuilders.TryAdd(file, new Lazy<(ControlBuilderDescriptor, Lazy<IControlBuilder>)>(() => (builder.Descriptor, new Lazy<IControlBuilder>(() => builder))));
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
