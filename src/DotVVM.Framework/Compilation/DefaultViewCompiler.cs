using System;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.Compilation
{
    public class ViewCompilerConfiguration
    {
        public List<Func<ResolvedControlTreeVisitor>> TreeVisitors { get; } = new List<Func<ResolvedControlTreeVisitor>>();
    }
    public class DefaultViewCompiler : IViewCompiler
    {
        public DefaultViewCompiler(IOptions<ViewCompilerConfiguration> config, IControlTreeResolver controlTreeResolver, IBindingCompiler bindingCompiler, Func<Validation.ControlUsageValidationVisitor> controlValidatorFactory, DotvvmMarkupConfiguration markupConfiguration, IClientModuleCompiler clientModuleCompiler)
        {
           this.config = config.Value;
           this.controlTreeResolver = controlTreeResolver;
           this.bindingCompiler = bindingCompiler;
           this.controlValidatorFactory = controlValidatorFactory;
           this.markupConfiguration = markupConfiguration;
           this.clientModuleCompiler = clientModuleCompiler;
        }

        private readonly IControlTreeResolver controlTreeResolver;
        private readonly IBindingCompiler bindingCompiler;
        private readonly ViewCompilerConfiguration config;
        private readonly Func<Validation.ControlUsageValidationVisitor> controlValidatorFactory;
        private readonly DotvvmMarkupConfiguration markupConfiguration;
        private readonly IClientModuleCompiler clientModuleCompiler;

        /// <summary>
        /// Compiles the view and returns a function that can be invoked repeatedly. The function builds full control tree and activates the page.
        /// </summary>
        private (ControlBuilderDescriptor, Func<Assembly>) CompileView(string sourceCode, MarkupFile file, CompilationBuilder compilationBuilder, string namespaceName, string className)
        {
            // parse the document
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(sourceCode);
            var parser = new DothtmlParser();
            var node = parser.Parse(tokenizer.Tokens);

            var resolvedView = (ResolvedTreeRoot)controlTreeResolver.ResolveTree(node, file);

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


                var validationVisitor = this.controlValidatorFactory.Invoke();
                validationVisitor.VisitAndAssert(resolvedView);

                var emitter = new DefaultViewCompilerCodeEmitter();
                var compilingVisitor = new ViewCompilingVisitor(emitter, bindingCompiler, clientModuleCompiler, className);

                resolvedView.Accept(compilingVisitor);

                var trees = emitter.BuildTree(namespaceName, className, file.FileName);
                compilationBuilder.AddToCompilation(trees, emitter.UsedAssemblies);
                return compilationBuilder.BuildAssembly();
            });
        }

        protected virtual IControlBuilder GetControlBuilder(Assembly assembly, string namespaceName, string className)
        {
            return (IControlBuilder)assembly.CreateInstance(namespaceName + "." + className);
        }

        public virtual (ControlBuilderDescriptor, Func<IControlBuilder>) CompileView(string sourceCode, MarkupFile file, string assemblyName, string namespaceName, string className)
        {
            var compilationBuilder = new CompilationBuilder(markupConfiguration, assemblyName);
            var (descriptor, assemblyBuilder) = CompileView(sourceCode, file, compilationBuilder, namespaceName, className);
            return (descriptor, () => GetControlBuilder(assemblyBuilder(), namespaceName, className));
        }
    }
}
