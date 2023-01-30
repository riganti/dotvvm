using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.Compilation.ViewCompiler
{
    public class DefaultViewCompiler : IViewCompiler
    {
        private readonly IControlTreeResolver controlTreeResolver;
        private readonly IBindingCompiler bindingCompiler;
        private readonly ViewCompilerConfiguration config;
        private readonly Func<Validation.ControlUsageValidationVisitor> controlValidatorFactory;

        public DefaultViewCompiler(IOptions<ViewCompilerConfiguration> config, IControlTreeResolver controlTreeResolver, IBindingCompiler bindingCompiler, Func<Validation.ControlUsageValidationVisitor> controlValidatorFactory)
        {
            this.config = config.Value;
            this.controlTreeResolver = controlTreeResolver;
            this.bindingCompiler = bindingCompiler;
            this.controlValidatorFactory = controlValidatorFactory;
        }

        /// <summary>
        /// Compiles the view and returns a function that can be invoked repeatedly. The function builds full control tree and activates the page.
        /// </summary>
        protected virtual (ControlBuilderDescriptor, Func<Func<IControlBuilderFactory, IServiceProvider, DotvvmControl>>) CompileViewCore(string sourceCode, string fileName)
        {
            // parse the document
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(sourceCode);
            var parser = new DothtmlParser();
            var node = parser.Parse(tokenizer.Tokens);

            var resolvedView = (ResolvedTreeRoot)controlTreeResolver.ResolveTree(node, fileName);

            // we have to throw on error before we return the descriptor, otherwise directive errors are ignored until
            // the control is invoked, so more cryptic error on the using page might be thrown.
            ThrowOnErrors(tokenizer.Tokens);
            ThrowOnErrors(node);

            var descriptor = resolvedView.ControlBuilderDescriptor;

            return (descriptor, () => {

                var errorCheckingVisitor = new ErrorCheckingVisitor();
                resolvedView.Accept(errorCheckingVisitor);

                // check errors again, because tree is lazy and the Accept call above forced compilation of the entire view
                ThrowOnErrors(tokenizer.Tokens);
                ThrowOnErrors(node);

                foreach (var visitor in config.TreeVisitors)
                    visitor().ApplyAction(resolvedView.Accept).ApplyAction(v => (v as IDisposable)?.Dispose());


                var validationVisitor = this.controlValidatorFactory.Invoke();
                validationVisitor.VisitAndAssert(resolvedView);

                var emitter = new DefaultViewCompilerCodeEmitter();
                var compilingVisitor = new ViewCompilingVisitor(emitter, bindingCompiler);

                resolvedView.Accept(compilingVisitor);

                return compilingVisitor.BuildCompiledView;
            }
            );
        }

        /// <summary> Recursively walks all nodes, throws on any NodeErrors </summary>
        private static void ThrowOnErrors(DothtmlNode node)
        {
            foreach (var n in node.EnumerateNodes())
            {
                if (n.HasNodeErrors)
                {
                    throw new DotvvmCompilationException(string.Join(", ", n.NodeErrors), n.Tokens);
                }
            }
        }

        private static void ThrowOnErrors(List<DothtmlToken> tokens)
        {
            foreach (var token in tokens)
            {
                if (token.Error is { IsCritical: true })
                {
                    throw new DotvvmCompilationException(token.Error.ErrorMessage, new[] { (token.Error as BeginWithLastTokenOfTypeTokenError<DothtmlToken, DothtmlTokenType>)?.LastToken ?? token });
                }
            }
        }

        public virtual (ControlBuilderDescriptor, Func<IControlBuilder>) CompileView(string sourceCode, string fileName)
        {
            var (descriptor, viewBuildingDelegateGetter) = CompileViewCore(sourceCode, fileName);
            return (descriptor, () => new DelegateControlBuilder(descriptor, viewBuildingDelegateGetter()));
        }
    }
}
