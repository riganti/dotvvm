using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Compiler
{
    public class StaticViewCompiler
    {
        public const string ObjectsClassName = "SerializedObjects";
        private const string IDotvvmCacheAdapterName
            = "DotVVM.Framework.Runtime.Caching.IDotvvmCacheAdapter, DotVVM.Framework";
        private const string SimpleDictionaryCacheAdapterName
            = "DotVVM.Framework.Testing.SimpleDictionaryCacheAdapter, DotVVM.Framework";
        private readonly ImmutableArray<MetadataReference> baseReferences;

        private readonly DotvvmConfiguration configuration;
        private readonly Assembly dotvvmProjectAssembly;

        // NB: Currently, an Assembly must be built for each view/markup control (i.e. IControlBuilder) and then merged
        //     into one assembly. It's horrible, I know, but the compiler is riddled with references to System.Type
        //     that make it presently impossible to compile it all in one go.
        private readonly ConcurrentDictionary<string, StaticView> viewCache
            = new ConcurrentDictionary<string, StaticView>();

        public StaticViewCompiler(DotvvmConfiguration configuration, Assembly dotvvmProjectAssembly)
        {
            this.configuration = configuration;
            this.dotvvmProjectAssembly = dotvvmProjectAssembly;
            baseReferences = GetBaseReferences(configuration);
        }

        public static DotvvmConfiguration CreateConfiguration(
            Assembly dotvvmProjectAssembly,
            string dotvvmProjectDir)
        {
            // ReplaceDefaultTypeRegistry(dotvvmProjectAssembly);
            ReplaceDefaultDependencyContext(dotvvmProjectAssembly);
            InitializeDotvvmControls(dotvvmProjectAssembly);
            return ConfigurationInitializer.GetConfiguration(dotvvmProjectAssembly, dotvvmProjectDir, services =>
            {
                services.AddSingleton<IControlResolver, StaticViewControlResolver>();
                services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>();
                services.AddSingleton(new RefObjectSerializer());
                // Yes, this is here so that there can be a circular dependency in StaticViewControlResolver.
                // I'm not happy about it, no, but the alternative is a more-or-less complete rewrite.
                services.AddSingleton(p => new StaticViewCompiler(
                    p.GetRequiredService<DotvvmConfiguration>(),
                    dotvvmProjectAssembly));

                // HACK: IDotvvmCacheAdapter is not in v2.0.0 that's why it's hacked this way.
                var iCacheAdapter = Type.GetType(IDotvvmCacheAdapterName);
                if (iCacheAdapter is object)
                {
                    services.AddSingleton(iCacheAdapter, Type.GetType(SimpleDictionaryCacheAdapterName));
                }

                // TODO: Uncomment when the views can actually be compiled into one assembly.
                // var bindingCompiler = new AssemblyBindingCompiler(
                //     assemblyName: null,
                //     className: null,
                //     outputFileName: null,
                //     configuration: null);
                // services.AddSingleton<IBindingCompiler>(bindingCompiler);
                // services.AddSingleton<IExpressionToDelegateCompiler>(bindingCompiler.GetExpressionToDelegateCompiler());
            });
        }

        public StaticView GetView(string viewPath)
        {
            if (viewCache.ContainsKey(viewPath))
            {
                return viewCache[viewPath];
            }

            var view = CompileView(viewPath);
            // NB: in the meantime, the view could have been compiled on another thread
            return viewCache.GetOrAdd(viewPath, view);
        }

        public IEnumerable<StaticView> CompileAllViews()
        {
            var markupControls = CompileViews(configuration.Markup.Controls.Select(c => c.Src));
            var views = CompileViews(configuration.RouteTable.Select(r => r.VirtualPath));
            return markupControls.Concat(views).ToImmutableArray();
        }

        private ImmutableArray<StaticView> CompileViews(IEnumerable<string?> paths)
        {
            return paths
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => GetView(p!))
                .ToImmutableArray();
        }

        private StaticView CompileView(string viewPath)
        {
            var fileLoader = configuration.ServiceProvider.GetRequiredService<IMarkupFileLoader>();
            var file = fileLoader.GetMarkup(configuration, viewPath);
            if (file is null)
            {
                throw new ArgumentException($"No view with path '{viewPath}' exists.");
            }

            // parse the document
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(file.ContentsReaderFactory());
            var parser = new DothtmlParser();
            var node = parser.Parse(tokenizer.Tokens);

            // analyze control types
            var controlTreeResolver = configuration.ServiceProvider.GetRequiredService<IControlTreeResolver>();
            var resolvedView = (ResolvedTreeRoot)controlTreeResolver.ResolveTree(node, viewPath);

            var view = new StaticView(viewPath);
            var reports = new List<Report>();

            try
            {
                var errorCheckingVisitor = new ErrorCheckingVisitor();
                resolvedView.Accept(errorCheckingVisitor);
            }
            catch (DotvvmCompilationException e)
            {
                reports.Add(new Report(viewPath, e));
                // the error is too severe for compilation to continue 
                return view.WithReports(reports);
            }

            foreach (var n in node.EnumerateNodes())
            {
                if (n.HasNodeErrors)
                {
                    var line = n.Tokens.FirstOrDefault()?.LineNumber ?? -1;
                    var column = n.Tokens.FirstOrDefault()?.ColumnNumber ?? -1;

                    reports.AddRange(n.NodeErrors.Select(e => new Report(viewPath, line, column, e)));
                    // these errors are once again too severe
                    return view.WithReports(reports);
                }
            }

            var contextSpaceVisitor = new DataContextPropertyAssigningVisitor();
            resolvedView.Accept(contextSpaceVisitor);

            var styleVisitor = new StylingVisitor(configuration);
            resolvedView.Accept(styleVisitor);

            var usageValidator = configuration.ServiceProvider.GetRequiredService<IControlUsageValidator>();
            var validationVisitor = new ControlUsageValidationVisitor(usageValidator);
            resolvedView.Accept(validationVisitor);
            foreach (var error in validationVisitor.Errors)
            {
                var line = error.Nodes.FirstOrDefault()?.Tokens?.FirstOrDefault()?.LineNumber ?? -1;
                var column = error.Nodes.FirstOrDefault()?.Tokens?.FirstOrDefault()?.ColumnNumber ?? -1;

                reports.Add(new Report(viewPath, line, column, error.ErrorMessage));
            }

            if (reports.Any())
            {
                return view.WithReports(reports);
            }

            // no dothtml compilation errors beyond this point

            // NOTE: Markup controls referenced in the view have already been compiled "thanks" to the circular
            //       dependency in StaticViewControlResolver.
            var namespaceName = DefaultControlBuilderFactory.GetNamespaceFromFileName(
                file.FileName,
                file.LastWriteDateTimeUtc);
            var className = DefaultControlBuilderFactory.GetClassFromFileName(file.FileName) + "ControlBuilder";
            string fullClassName = namespaceName + "." + className;
            var refObjectSerializer = configuration.ServiceProvider.GetRequiredService<RefObjectSerializer>();
            var emitter = new DefaultViewCompilerCodeEmitter();
            var assemblyCache = configuration.ServiceProvider.GetRequiredService<CompiledAssemblyCache>();
            var bindingCompiler = configuration.ServiceProvider.GetRequiredService<IBindingCompiler>();
            var compilingVisitor = new ViewCompilingVisitor(emitter, assemblyCache, bindingCompiler, className);
            resolvedView.Accept(compilingVisitor);

            // HACK: In 2.4.0, ReflectionUtils.FindType expects to find the controls in the current assembly,
            //       which is bollocks.
            if (resolvedView.Metadata.Type != typeof(DotvvmView))
            {
                emitter.ResultControlTypeSyntax = emitter.ParseTypeName(resolvedView.Metadata.Type);
            }

            if (resolvedView.Directives.ContainsKey("masterPage"))
            {
                // make sure that the masterpage chain is already compiled
                _ = GetView(resolvedView.Directives["masterPage"].Single().Value);
            }

            var syntaxTree = emitter.BuildTree(namespaceName, className, viewPath).Single();
            var references = emitter.UsedAssemblies.Select(a =>
                MetadataReference.CreateFromFile(a.Key.Location).WithAliases(new[] { a.Value, "global" }));

            var compilation = CSharpCompilation.Create(
                assemblyName: $"{fullClassName}.Compiled",
                syntaxTrees: new[] { syntaxTree },
                references: baseReferences.Concat(references),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var memoryStream = new MemoryStream();
            var result = compilation.Emit(memoryStream);
            if (!result.Success)
            {
                reports.Add(new Report(viewPath, -1, -1, "Compilation failed. This is likely a bug in the DotVVM compiler."));
                return view.WithReports(reports);
            }

            var assembly = Assembly.Load(memoryStream.ToArray());
            return view.WithAssembly(assembly)
                .WithViewType(resolvedView.Metadata.Type)
                .WithDataContextType(resolvedView.DataContextTypeStack.DataContextType);
        }

        /// <summary>
        /// HACK: Because as of 2.4.0, DotVVM gets a list of all assemblies only from the Default DependencyContext,
        ///       a problem arises, because in this Compiler, DotVVM itself isn't in the Default DependencyContext, thus
        ///       I need to invoke the static constructors of controls myself.
        /// </summary>
        private static void InitializeDotvvmControls(Assembly rootAssembly)
        {
            var dotvvmAssemblyName = typeof(DotvvmControl).Assembly.GetName().Name;

            var candidateAssemblies = rootAssembly.GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Where(a => a.GetReferencedAssemblies().Any(s => s.Name == dotvvmAssemblyName))
                .ToArray();

            var types = candidateAssemblies
                .Concat(new[] { typeof(DotvvmControl).Assembly })
                .SelectMany(a => a.GetLoadableTypes()
                    .Where(t => t.IsClass && t.GetCustomAttribute<ContainsDotvvmPropertiesAttribute>() is object))
                .ToArray();

            foreach (var type in types)
            {
                var tt = type;
                do
                {
                    RuntimeHelpers.RunClassConstructor(tt.TypeHandle);
                    tt = tt.GetTypeInfo().BaseType;
                }
                while (tt != null && tt.GetTypeInfo().IsGenericType);
            }
        }

        private static void ReplaceDefaultDependencyContext(Assembly projectAssembly)
        {
#if NET461
            return;
#else
            var projectContext = Microsoft.Extensions.DependencyModel.DependencyContext.Load(projectAssembly);
            var mergedContext = Microsoft.Extensions.DependencyModel.DependencyContext.Default.Merge(projectContext);
            var fields = typeof(Microsoft.Extensions.DependencyModel.DependencyContext)
                 .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)!;
            foreach (var field in fields)
            {
                field.SetValue(
                    Microsoft.Extensions.DependencyModel.DependencyContext.Default,
                    field.GetValue(mergedContext));
            }
#endif
        }

        private static ImmutableArray<MetadataReference> GetBaseReferences(DotvvmConfiguration configuration)
        {
            // TODO: This method is a dupe of a part of DefaultViewCompiler
            var diAssembly = typeof(ServiceCollection).Assembly;
            var builder = ImmutableArray.CreateBuilder<Assembly>();
            builder.AddRange(diAssembly.GetReferencedAssemblies().Select(Assembly.Load));
            builder.Add(diAssembly);
            builder.AddRange(configuration.Markup.Assemblies.Select(n => Assembly.Load(new AssemblyName(n))));
            builder.Add(Assembly.Load(new AssemblyName("mscorlib")));
            builder.Add(Assembly.Load(new AssemblyName("System.ValueTuple")));
            builder.Add(typeof(IServiceProvider).Assembly);
            builder.Add(typeof(RuntimeBinderException).Assembly);
            builder.Add(typeof(DynamicAttribute).Assembly);
            builder.Add(typeof(DotvvmConfiguration).Assembly);
#if NETCOREAPP3_1
            builder.Add(Assembly.Load(new AssemblyName("System.Runtime")));
            builder.Add(Assembly.Load(new AssemblyName("System.Collections.Concurrent")));
            builder.Add(Assembly.Load(new AssemblyName("System.Collections")));
#elif NET461
            builder.Add(typeof(List<>).Assembly);

#else
#error Fix TargetFrameworks.
#endif

            var netstandardAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "netstandard");
            if (netstandardAssembly is object)
            {
                builder.Add(netstandardAssembly);
            }
            else
            {
                try
                {
                    // netstandard assembly is required for netstandard 2.0 and in some cases
                    // for netframework461 and newer. netstandard is not included in netframework452
                    // and will throw FileNotFoundException. Instead of detecting current netframework
                    // version, the exception is swallowed.
                    builder.Add(Assembly.Load(new AssemblyName("netstandard")));
                }
                catch (FileNotFoundException) { }
            }

            return builder.Select(a => (MetadataReference)MetadataReference.CreateFromFile(a.Location))
                .ToImmutableArray();
        }
    }
}
