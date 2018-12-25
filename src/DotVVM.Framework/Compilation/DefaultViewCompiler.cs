using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp.RuntimeBinder;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.Compilation
{
    public class ViewCompilerConfiguration
    {
        public List<Func<ResolvedControlTreeVisitor>> TreeVisitors { get; } = new List<Func<ResolvedControlTreeVisitor>>();
    }
    public class DefaultViewCompiler : IViewCompiler
    {
        public DefaultViewCompiler(IOptions<ViewCompilerConfiguration> config, IControlTreeResolver controlTreeResolver, IBindingCompiler bindingCompiler, Func<Validation.ControlUsageValidationVisitor> controlValidatorFactory, DotvvmMarkupConfiguration markupConfiguration)
        {
           this.config = config.Value;
           this.controlTreeResolver = controlTreeResolver;
           this.bindingCompiler = bindingCompiler;
           this.assemblyCache = CompiledAssemblyCache.Instance;
           this.controlValidatorFactory = controlValidatorFactory;
           this.markupConfiguration = markupConfiguration;
        }

        private readonly CompiledAssemblyCache assemblyCache;
        private readonly IControlTreeResolver controlTreeResolver;
        private readonly IBindingCompiler bindingCompiler;
        private readonly ViewCompilerConfiguration config;
        private readonly Func<Validation.ControlUsageValidationVisitor> controlValidatorFactory;
        private readonly DotvvmMarkupConfiguration markupConfiguration;

        /// <summary>
        /// Compiles the view and returns a function that can be invoked repeatedly. The function builds full control tree and activates the page.
        /// </summary>
        public virtual (ControlBuilderDescriptor, Func<CSharpCompilation>) CompileView(string sourceCode, string fileName, CSharpCompilation compilation, string namespaceName, string className)
        {
            // parse the document
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(sourceCode);
            var parser = new DothtmlParser();
            var node = parser.Parse(tokenizer.Tokens);

            var resolvedView = (ResolvedTreeRoot)controlTreeResolver.ResolveTree(node, fileName);

            return (new ControlBuilderDescriptor(resolvedView.DataContextTypeStack.DataContextType, resolvedView.Metadata.Type), () => {

                var errorCheckingVisitor = new ErrorCheckingVisitor();
                resolvedView.Accept(errorCheckingVisitor);

                foreach (var token in tokenizer.Tokens)
                {
                    if (token.HasError && token.Error.IsCritical)
                    {
                        throw new DotvvmCompilationException(token.Error.ErrorMessage, new[] { (token.Error as BeginWithLastTokenOfTypeTokenError<DothtmlToken, DothtmlTokenType>)?.LastToken ?? token });
                    }
                }

                foreach (var n in node.EnumerateNodes())
                {
                    if (n.HasNodeErrors)
                    {
                        throw new DotvvmCompilationException(string.Join(", ", n.NodeErrors), n.Tokens);
                    }
                }

                foreach (var visitor in config.TreeVisitors)
                    visitor().ApplyAction(resolvedView.Accept).ApplyAction(v => (v as IDisposable)?.Dispose());


                var validationVisitor = this.controlValidatorFactory.Invoke()
                    .ApplyAction(resolvedView.Accept);
                if (validationVisitor.Errors.Any())
                {
                    var controlUsageError = validationVisitor.Errors.First();
                    throw new DotvvmCompilationException(controlUsageError.ErrorMessage, controlUsageError.Nodes.SelectMany(n => n.Tokens));
                }

                var emitter = new DefaultViewCompilerCodeEmitter();
                var compilingVisitor = new ViewCompilingVisitor(emitter, bindingCompiler, className);

                resolvedView.Accept(compilingVisitor);

                return AddToCompilation(compilation, emitter, fileName, namespaceName, className);
            }
            );
        }

        protected virtual CSharpCompilation AddToCompilation(CSharpCompilation compilation, DefaultViewCompilerCodeEmitter emitter, string fileName, string namespaceName, string className)
        {
            var tree = emitter.BuildTree(namespaceName, className, fileName);
            return compilation
                .AddSyntaxTrees(tree)
                .AddReferences(emitter.UsedAssemblies
                    .Select(a => GetAssemblyCache().GetAssemblyMetadata(a.Key).WithAliases(ImmutableArray.Create(a.Value, "global"))));
        }

        private CompiledAssemblyCache GetAssemblyCache()
        {
            return assemblyCache;
        }

        public virtual CSharpCompilation CreateCompilation(string assemblyName)
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
#else
                    typeof(List<>).Assembly
#endif
                });

            try
            {
                // netstandard assembly is required for netstandard 2.0 and in some cases
                // for netframework461 and newer. netstandard is not included in netframework452
                // and will throw FileNotFoundException. Instead of detecting current netframework
                // version, the exception is swallowed.
                references = references.Concat(new[] { Assembly.Load(new AssemblyName("netstandard")) });
            }
            catch (FileNotFoundException) { }

            return CSharpCompilation.Create(assemblyName, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references.Distinct().Select(a => assemblyCache.GetAssemblyMetadata(a)));
        }

        protected virtual IControlBuilder GetControlBuilder(Assembly assembly, string namespaceName, string className)
        {
            return (IControlBuilder)assembly.CreateInstance(namespaceName + "." + className);
        }

        /// <summary>
        /// Builds the assembly.
        /// </summary>
        protected virtual Assembly BuildAssembly(CSharpCompilation compilation)
        {
            using (var ms = new MemoryStream())
            {
                Console.WriteLine("Compiling view:\n" + compilation.SyntaxTrees[0].GetRoot().NormalizeWhitespace());
                var result = compilation.Emit(ms);
                if (result.Success)
                {
                    var assembly = AssemblyLoader.LoadRaw(ms.ToArray());
                    assemblyCache.AddAssembly(assembly, compilation.ToMetadataReference());
                    return assembly;
                }
                else
                {
                    throw new Exception("The compilation failed! This is most probably bug in the DotVVM framework.\r\n\r\n"
                        + string.Join("\r\n", result.Diagnostics)
                        + "\r\n\r\n" + compilation.SyntaxTrees[0].GetRoot().NormalizeWhitespace() + "\r\n\r\n"
                        + "References: " + string.Join("\r\n", compilation.ReferencedAssemblyNames.Select(n => n.Name)));
                }
            }
        }

        public virtual (ControlBuilderDescriptor, Func<IControlBuilder>) CompileView(string sourceCode, string fileName, string assemblyName, string namespaceName, string className)
        {
            var compilation = CreateCompilation(assemblyName);
            var (descriptor, compilationGetter) = CompileView(sourceCode, fileName, compilation, namespaceName, className);
            return (descriptor, () => {
                var assembly = BuildAssembly(compilationGetter());
                return GetControlBuilder(assembly, namespaceName, className);
            }
            );
        }
    }
}
