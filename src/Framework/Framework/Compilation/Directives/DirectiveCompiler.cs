using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation.Directives
{
    public abstract class DirectiveCompiler<TDirective, TArtefact> : DirectiveResolver<TDirective>
        where TDirective : IAbstractDirective
    {
        public abstract string DirectiveName { get; }
        protected IAbstractTreeBuilder TreeBuilder { get; }

        public DirectiveCompiler(IReadOnlyDictionary<string, IReadOnlyList<DothtmlDirectiveNode>> directiveNodesByName, IAbstractTreeBuilder treeBuilder)
            : base(directiveNodesByName)
        {
            TreeBuilder = treeBuilder;
        }

        public DirectiveCompilationResult Compile()
        {
            var resolvedDirectives = Resolve(DirectiveName);
            return new DirectiveCompilationResult(
                    resolvedDirectives,
                    CreateArtefact(resolvedDirectives)
                );
        }

        protected abstract TArtefact CreateArtefact(IReadOnlyList<TDirective> resolvedDirectives);

        protected BindingParserNode ParseDirective(DothtmlDirectiveNode directiveNode, Func<BindingParser, BindingParserNode> parserFunc)
        {
            var tokenizer = new BindingTokenizer();
            tokenizer.Tokenize(directiveNode.ValueNode.Text);
            var parser = new BindingParser() {
                Tokens = tokenizer.Tokens
            };
            var valueSyntaxRoot = parserFunc(parser);
            if (!parser.OnEnd())
            {
                directiveNode.AddError($"Unexpected token: {parser.Peek()?.Text}.");
            }
            return valueSyntaxRoot;
        }

        public record DirectiveCompilationResult(ImmutableList<TDirective> Directives, TArtefact Artefact);
    }

}
