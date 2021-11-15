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
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation
{
    public class ViewCompilerConfiguration
    {
        public List<Func<ResolvedControlTreeVisitor>> TreeVisitors { get; } = new List<Func<ResolvedControlTreeVisitor>>();
    }
    public class DefaultViewCompiler : IViewCompiler
    {
        public DefaultViewCompiler(IOptions<ViewCompilerConfiguration> config, IControlTreeResolver controlTreeResolver, IBindingCompiler bindingCompiler, Func<Validation.ControlUsageValidationVisitor> controlValidatorFactory, CompiledAssemblyCache compiledAssemblyCache)
        {
            this.config = config.Value;
            this.controlTreeResolver = controlTreeResolver;
            this.bindingCompiler = bindingCompiler;
            this.controlValidatorFactory = controlValidatorFactory;
            this.assemblyCache = compiledAssemblyCache;
        }

        private readonly CompiledAssemblyCache assemblyCache;
        private readonly IControlTreeResolver controlTreeResolver;
        private readonly IBindingCompiler bindingCompiler;
        private readonly ViewCompilerConfiguration config;
        private readonly Func<Validation.ControlUsageValidationVisitor> controlValidatorFactory;

        /// <summary>
        /// Compiles the view and returns a function that can be invoked repeatedly. The function builds full control tree and activates the page.
        /// </summary>
        public virtual (ControlBuilderDescriptor, Func<Delegate>) CompileView(string sourceCode, string fileName, string namespaceName, string className)
        {
            // parse the document
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(sourceCode);
            var parser = new DothtmlParser();
            var node = parser.Parse(tokenizer.Tokens);

            var resolvedView = (ResolvedTreeRoot)controlTreeResolver.ResolveTree(node, fileName);

            var descriptor = resolvedView.ControlBuilderDescriptor;

            return (descriptor, () => {

                var errorCheckingVisitor = new ErrorCheckingVisitor();
                resolvedView.Accept(errorCheckingVisitor);

                foreach (var token in tokenizer.Tokens)
                {
                    if (token.Error is { IsCritical: true })
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


                var validationVisitor = this.controlValidatorFactory.Invoke();
                validationVisitor.VisitAndAssert(resolvedView);

                var emitter = new DefaultViewCompilerCodeEmitter();
                var compilingVisitor = new ViewCompilingVisitor(emitter, assemblyCache, bindingCompiler, className);

                resolvedView.Accept(compilingVisitor);

                return compilingVisitor.CompiledViewDelegate;
            }
            );
        }

        /// <summary>
        /// Builds the assembly.
        /// </summary>
        protected virtual Assembly BuildAssembly(CSharpCompilation compilation)
        {
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (result.Success)
                {
                    var assembly = AssemblyLoader.LoadRaw(ms.ToArray());
                    assemblyCache.AddAssemblyMetadata(assembly, compilation.ToMetadataReference());
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
            var (descriptor, viewBuildingDelegateGetter) = CompileView(sourceCode, fileName, namespaceName, className);
            return (descriptor, () => new DelegateControlBuilder(descriptor, (Func<IControlBuilderFactory, IServiceProvider, DotvvmControl>)viewBuildingDelegateGetter()));
        }
    }

    public record DelegateControlBuilder : IControlBuilder
    {
        private readonly Func<IControlBuilderFactory, IServiceProvider, DotvvmControl> controlBuilderDelegate;

        public DelegateControlBuilder(ControlBuilderDescriptor controlBuilderDescriptor, Func<IControlBuilderFactory, IServiceProvider, DotvvmControl> controlBuilderDelegate)
        {
            Descriptor = controlBuilderDescriptor;
            this.controlBuilderDelegate = controlBuilderDelegate;
        }

        public ControlBuilderDescriptor Descriptor { get; }

        public DotvvmControl BuildControl(IControlBuilderFactory controlBuilderFactory, IServiceProvider services)
        {
            return controlBuilderDelegate(controlBuilderFactory, services);
        }
    }
}
