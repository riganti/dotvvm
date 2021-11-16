﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation
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
                var compilingVisitor = new ViewCompilingVisitor(emitter, bindingCompiler);

                resolvedView.Accept(compilingVisitor);

                return compilingVisitor.CompiledViewDelegate;
            }
            );
        }

        public virtual (ControlBuilderDescriptor, Func<IControlBuilder>) CompileView(string sourceCode, string fileName, string assemblyName, string namespaceName, string className)
        {
            var (descriptor, viewBuildingDelegateGetter) = CompileView(sourceCode, fileName, namespaceName, className);
            return (descriptor, () => new DelegateControlBuilder(descriptor, (Func<IControlBuilderFactory, IServiceProvider, DotvvmControl>)viewBuildingDelegateGetter()));
        }
    }
}