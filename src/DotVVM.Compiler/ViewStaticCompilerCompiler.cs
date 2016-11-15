using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation.Validation;

namespace DotVVM.Compiler
{
    class ViewStaticCompilerCompiler
    {
        private static ConcurrentDictionary<string, Assembly> assemblyDictionary = new ConcurrentDictionary<string, Assembly>();
        private static ConcurrentDictionary<string, DotvvmConfiguration> cachedConfig = new ConcurrentDictionary<string, DotvvmConfiguration>();

        public CompilerOptions Options { get; set; }
        private DotvvmConfiguration configuration;
        AssemblyBindingCompiler bindingCompiler;
        IControlTreeResolver controlTreeResolver;
        IViewCompiler compiler;
        IMarkupFileLoader fileLoader;
        CSharpCompilation compilation;
        CompilationResult result = new CompilationResult();

        private void InitOptions()
        {
            if (Options.OutputPath == null) Options.OutputPath = "./output";
            if (Options.AssemblyName == null) Options.AssemblyName = "CompiledViews";
            if (Options.BindingsAssemblyName == null) Options.BindingsAssemblyName = Options.AssemblyName + "Bindings";
            if (Options.BindingClassName == null) Options.BindingClassName = Options.BindingsAssemblyName + "." + "CompiledBindings";
        }

        DotvvmConfiguration GetCachedConfiguration(Assembly assembly, string webSitePath)
            => cachedConfig.GetOrAdd($"{assembly.GetName().Name}|{webSitePath}", key =>
            {
                return OwinInitializer.InitDotVVM(assembly, webSitePath, bindingCompiler, this);
            });

        private void Init()
        {
            // touch assembly
            SyntaxFactory.Token(SyntaxKind.NullKeyword);

            InitOptions();
            if (Options.FullCompile)
                if (!Directory.Exists(Options.OutputPath)) Directory.CreateDirectory(Options.OutputPath);
            var wsa = assemblyDictionary.GetOrAdd(Options.WebSiteAssembly, _ => Assembly.LoadFile(Options.WebSiteAssembly));
            configuration = GetCachedConfiguration(wsa, Options.WebSitePath);
            bindingCompiler = new AssemblyBindingCompiler(Options.BindingsAssemblyName, Options.BindingClassName, Path.Combine(Options.OutputPath, Options.BindingsAssemblyName + ".dll"), configuration);
            if (Options.DothtmlFiles == null) Options.DothtmlFiles = configuration.RouteTable.Select(r => r.VirtualPath).ToArray();
            controlTreeResolver = configuration.ServiceLocator.GetService<IControlTreeResolver>();
            fileLoader = configuration.ServiceLocator.GetService<IMarkupFileLoader>();
            if (Options.FullCompile)
            {
                compiler = configuration.ServiceLocator.GetService<IViewCompiler>();
                compilation = compiler.CreateCompilation(Options.AssemblyName);
            }

            if (Options.SerializeConfig)
            {
                result.Configuration = configuration;
            }

            // touch assemblies
        }

        private void Save()
        {
            if (Options.FullCompile)
            {
                var bindingsAssemblyPath = bindingCompiler.OutputFileName;
                bindingCompiler.SaveAssembly();
                Program.WriteInfo("bindings saved to " + bindingsAssemblyPath);
                compilation = compilation.AddReferences(MetadataReference.CreateFromFile(Path.GetFullPath(bindingsAssemblyPath)));
                var compiledViewsFileName = Path.Combine(Options.OutputPath, Options.AssemblyName + ".dll");
                //Directory.CreateDirectory("outputCS");
                //int i = 0;
                //foreach (var tree in compilation.SyntaxTrees)
                //{
                //    File.WriteAllText($"outputCS/file{i++}.cs", tree.GetRoot().NormalizeWhitespace().ToString());
                //}
                var result = compilation.Emit(compiledViewsFileName);
                if (!result.Success)
                {
                    throw new Exception("compilation failed");
                }
                Program.WriteInfo("views saved to " + compiledViewsFileName);
            }
        }

        public CompilationResult Execute()
        {
            Program.WriteInfo("loading");
            Init();
            Program.WriteInfo("compiling views");
            foreach (var file in Options.DothtmlFiles)
            {
                try
                {
                    CompileFile(file);
                }
                catch (DotvvmCompilationException exception)
                {
                    result.Files.Add(file, new FileCompilationResult
                    {
                        Errors = new List<Exception>() { exception }
                    });
                }
            }
            Program.WriteInfo("saving assemblies");
            Save();
            Program.WriteInfo("building results");
            return result;
        }

        void BuildFileResult(string fileName, ViewCompilationResult view)
        {
            var r = new FileCompilationResult();
            var visitor = new ResolvedControlInfoVisitor();
            if (Options.CheckBindingErrors) visitor.BindingCompiler = bindingCompiler;
            visitor.Result = r;
            view.ResolvedTree?.Accept(visitor);
            result.Files.Add(fileName, r);
        }

        private Dictionary<string, ViewCompilationResult> compiledCache = new Dictionary<string, ViewCompilationResult>();
        public ViewCompilationResult CompileFile(string fileName)
        {
            if (compiledCache.ContainsKey(fileName)) return compiledCache[fileName];
            return compiledCache[fileName] = CompileView(fileName);
        }

        protected ViewCompilationResult CompileView(string fileName)
        {
            var file = fileLoader.GetMarkup(configuration, fileName);

            // parse the document
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(file.ContentsReaderFactory());
            var parser = new DothtmlParser();
            var node = parser.Parse(tokenizer.Tokens);

            var resolvedView = (ResolvedTreeRoot)controlTreeResolver.ResolveTree(node, fileName);

            var errorCheckingVisitor = new ErrorCheckingVisitor();
            resolvedView.Accept(errorCheckingVisitor);


            foreach (var n in node.EnumerateNodes())
            {
                if (n.HasNodeErrors)
                {
                    throw new DotvvmCompilationException(string.Join(", ", n.NodeErrors), n.Tokens);
                }
            }

            var styleVisitor = new StylingVisitor(configuration.Styles);
            resolvedView.Accept(styleVisitor);

            var validationVisitor = new ControlUsageValidationVisitor(configuration);
            resolvedView.Accept(validationVisitor);
            if (validationVisitor.Errors.Any())
            {
                var controlUsageError = validationVisitor.Errors.First();
                throw new DotvvmCompilationException(controlUsageError.ErrorMessage, controlUsageError.Nodes.SelectMany(n => n.Tokens));
            }

            DefaultViewCompilerCodeEmitter emitter = null;
            string fullClassName = null;
            if (Options.FullCompile)
            {
                var namespaceName = DefaultControlBuilderFactory.GetNamespaceFromFileName(file.FileName, file.LastWriteDateTimeUtc);
                var className = DefaultControlBuilderFactory.GetClassFromFileName(file.FileName) + "ControlBuilder";
                fullClassName = namespaceName + "." + className;
                emitter = new DefaultViewCompilerCodeEmitter();
                var compilingVisitor = new ViewCompilingVisitor(emitter, configuration.ServiceLocator.GetService<IBindingCompiler>(), className, b => configuration.ServiceLocator.GetService<IBindingIdGenerator>().GetId(b, fileName));

                resolvedView.Accept(compilingVisitor);

                // compile master pages
                if (resolvedView.Directives.ContainsKey("masterPage"))
                    CompileFile(resolvedView.Directives["masterPage"].Single().Value);

                compilation = compilation
                    .AddSyntaxTrees(emitter.BuildTree(namespaceName, className, fileName)/*.Select(t => SyntaxFactory.ParseSyntaxTree(t.GetRoot().NormalizeWhitespace().ToString()))*/)
                    .AddReferences(emitter.UsedAssemblies
                        .Select(a => CompiledAssemblyCache.Instance.GetAssemblyMetadata(a)));
            }

            Program.WriteInfo($"view { fileName } compiled");

            var res = new ViewCompilationResult
            {
                BuilderClassName = fullClassName,
                ControlType = resolvedView.Metadata.Type,
                DataContextType = emitter?.BuilderDataContextType,
                ResolvedTree = Options.OutputResolvedDothtmlMap ? resolvedView : null
            };
            BuildFileResult(fileName, res);
            return res;
        }
    }

    class ViewCompilationResult
    {
        public string BuilderClassName { get; set; }
        public Type ControlType { get; set; }
        public Type DataContextType { get; set; }
        public ResolvedTreeRoot ResolvedTree { get; set; }
    }
}
