using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Immutable;
using DotVVM.Framework.Utils;
using System.Runtime.Loader;

namespace DotVVM.Compiler.Blazor
{
    internal class Dothtml2BlazorCompiler
    {
        static ConcurrentDictionary<string, Assembly> assemblyDictionary = new ConcurrentDictionary<string, Assembly>();
        static ConcurrentDictionary<string, DotvvmConfiguration> cachedConfig = new ConcurrentDictionary<string, DotvvmConfiguration>();

        public CompilerOptions Options { get; }
        readonly DotvvmConfiguration configuration;
        readonly IControlTreeResolver controlTreeResolver;
        readonly IViewCompiler compiler;
        readonly IMarkupFileLoader fileLoader;
        readonly CompilationResult result = new CompilationResult();
        CSharpCompilation compilation;

        public Dothtml2BlazorCompiler(DotvvmConfiguration dotvvmConfiguration, CompilerOptions options)
        {
            this.configuration = dotvvmConfiguration;
            this.Options = options;

            controlTreeResolver = configuration.ServiceProvider.GetRequiredService<IControlTreeResolver>();
            fileLoader = configuration.ServiceProvider.GetRequiredService<IMarkupFileLoader>();
            compiler = configuration.ServiceProvider.GetRequiredService<IViewCompiler>();
            compilation = CreateCompilation(Options.AssemblyName);
            // compilation = compilation;
        }

        public static CSharpCompilation CreateCompilation(string assemblyName)
        {
            var references =
                typeof(DotvvmConfiguration).Assembly.GetReferencedAssemblies().Select(Assembly.Load);

            return CSharpCompilation.Create(assemblyName, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references.Select(a => CompiledAssemblyCache.Instance.GetAssemblyMetadata(a)))
                .AddReferences(new [] { MetadataReference.CreateFromFile("/home/exyi/.nuget/packages/netstandard.library/2.0.1/build/netstandard2.0/ref/netstandard.dll") });
        }

        // private void Init()
        // {
        //     if (Options.FullCompile)
        //     {
        //         // touch assembly
        //         SyntaxFactory.Token(SyntaxKind.NullKeyword);

        //         if (!Directory.Exists(Options.OutputPath))
        //         {
        //             Directory.CreateDirectory(Options.OutputPath);
        //         }
        //     }

        //     // var wsa = assemblyDictionary.GetOrAdd(Options.WebSiteAssembly, _ => Assembly.LoadFile(Options.WebSiteAssembly));
        //     // AssemblyResolver.LoadReferences(wsa);

        //     if (Options.SerializeConfig)
        //     {
        //         result.Configuration = configuration;
        //     }
        // }

        private void Save()
        {
            if (Options.FullCompile)
            {
                // var bindingsAssemblyPath = bindingCompiler.OutputFileName;
                // bindingCompiler.SaveAssembly();

                // Program.WriteInfo($"Bindings saved to {bindingsAssemblyPath}.");

                // var compilation = this.compilation.AddReferences(MetadataReference.CreateFromFile(Path.GetFullPath(bindingsAssemblyPath)));

                compilation = compilation.WithOptions(compilation.Options.WithMainTypeName($"{Options.AssemblyName}.Program").WithOutputKind(OutputKind.ConsoleApplication));

                if (!Directory.Exists(Path.GetDirectoryName(Options.OutputPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(Options.OutputPath));
                var compiledViewsFileName = Path.Combine(Options.OutputPath, Options.AssemblyName + ".dll");

                foreach(var tree in compilation.SyntaxTrees)
                {
                    Console.WriteLine("     ---------------------------");
                    Console.WriteLine(tree.ToString());
                }
                Console.WriteLine();

                foreach(var rf in compilation.References)
                {
                    Console.WriteLine($"{rf.Display} - {string.Join(", ", rf.Properties.Aliases)}");
                }
                Console.WriteLine();

                try
                {
                    using (var outfile = File.Create(compiledViewsFileName))
                    {
                        var result = compilation.Emit(outfile);//, options: new Microsoft.CodeAnalysis.Emit.EmitOptions(runtimeMetadataVersion: "v4.0.31019"));
                        if (!result.Success)
                        {
                            Console.WriteLine("View compilation failed:");
                            foreach(var group in result.Diagnostics.GroupBy(d => d.Location.SourceTree))
                            {
                                Console.WriteLine("     ---------------------------");
                                Console.WriteLine(group.Key?.ToString());
                                foreach (var error in group)
                                    Console.WriteLine(error.ToString());
                                Console.WriteLine("     ---------------------------");
                                Console.WriteLine();
                            }
                            throw new Exception("The compilation failed!");
                        }
                        Program.WriteInfo($"Compiled views saved to {compiledViewsFileName}.");
                    }
                }
                catch(Exception error)
                {
                    Console.WriteLine("Could not compile generated views:");
                    Console.WriteLine(error);
                    foreach(var tree in compilation.SyntaxTrees)
                    {
                        Console.WriteLine("     ---------------------------");
                        Console.WriteLine(tree.ToString());
                    }
                    Console.WriteLine();
                    throw;
                }
            }
        }

        public CompilationResult Execute()
        {
            Program.WriteInfo("Initializing...");
            // Init();

            Options.PopulateRouteTable(this.configuration);

            Program.WriteInfo("Compiling views...");
            var compiledFiles = new List<(string file, ViewCompilationResult)>();
            foreach (var file in Options.DothtmlFiles.Distinct())
            {
                try
                {
                    var result = CompileFile(file);
                    compiledFiles.Add((file, result));
                }
                catch (DotvvmCompilationException exception)
                {
                    Console.WriteLine($"Error compiling view {file}:");
                    Console.WriteLine(exception);
                    result.Files.Add(file, new FileCompilationResult
                    {
                        Errors = new List<Exception>() { exception }
                    });
                }
            }

            var emitter = BlazorEntrypoint.CreateEntryPoint(this.Options.AssemblyName, compiledFiles.ToArray());
            AddEmitter("IDK_JUST_SOME_FILE", emitter,Options.AssemblyName, "Program");

            Program.WriteInfo("Emitting assemblies...");
            Save();

            Program.WriteInfo("Building compilation results...");
            return result;
        }

        private void BuildFileResult(string fileName, ViewCompilationResult view)
        {
            var r = new FileCompilationResult();
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

            var styleVisitor = new StylingVisitor(configuration);
            resolvedView.Accept(styleVisitor);

            var validationVisitor = new ControlUsageValidationVisitor(new DefaultControlUsageValidator());
            resolvedView.Accept(validationVisitor);
            if (validationVisitor.Errors.Any())
            {
                var controlUsageError = validationVisitor.Errors.First();
                throw new DotvvmCompilationException(controlUsageError.ErrorMessage, controlUsageError.Nodes.SelectMany(n => n.Tokens));
            }

            new LifecycleRequirementsAssigningVisitor().ApplyAction(resolvedView.Accept);


            var emitter = new BetterCodeEmitter();
            string fullClassName = null;
            if (Options.FullCompile)
            {
                var namespaceName = DefaultControlBuilderFactory.GetNamespaceFromFileName(file.FileName, file.LastWriteDateTimeUtc);
                var className = DefaultControlBuilderFactory.GetClassFromFileName(file.FileName) + "_BlazorComponent";
                fullClassName = namespaceName + "." + className;

                var compilingVisitor = new BlazorCompilingVisitor(emitter);
                resolvedView.Accept(compilingVisitor);

                // compile master pages
                // if (resolvedView.Directives.ContainsKey("masterPage"))
                //     CompileFile(resolvedView.Directives["masterPage"].Single().Value);
                AddEmitter(fileName, emitter, namespaceName, className);
            }

            Program.WriteInfo($"The view { fileName } compiled successfully.");

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

        private void AddEmitter(string fileName, BetterCodeEmitter emitter, string namespaceName, string className)
        {
            var trees = emitter.BuildTree(namespaceName, className, fileName).Select(t => t.WithRootAndOptions(t.GetRoot().NormalizeWhitespace(), t.Options)).ToArray();

            compilation = compilation
                .AddSyntaxTrees(trees)
                .AddReferences(emitter.UsedAssemblies
                    .Select(a => CompiledAssemblyCache.Instance.GetAssemblyMetadata(a.Key).WithAliases(ImmutableArray.Create(a.Value))));
        }

        void AddEmitter(DefaultViewCompilerCodeEmitter emitter)
        {

        }
    }

    internal class ViewCompilationResult
    {
        public string BuilderClassName { get; set; }
        public Type ControlType { get; set; }
        public Type DataContextType { get; set; }
        public ResolvedTreeRoot ResolvedTree { get; set; }
    }
}
