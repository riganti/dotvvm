using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation.Directives
{
    public class ImportDirectiveCompiler : DirectiveCompiler<IAbstractImportDirective, ImmutableList<NamespaceImport>>
    {
        public override string DirectiveName => ParserConstants.ImportNamespaceDirective;

        public ImportDirectiveCompiler(IReadOnlyDictionary<string, IReadOnlyList<DothtmlDirectiveNode>> directiveNodesByName, IAbstractTreeBuilder treeBuilder)
            : base(directiveNodesByName, treeBuilder)
        {
        }

        protected override IAbstractImportDirective Resolve(DothtmlDirectiveNode directiveNode)
        {
            var valueSyntaxRoot = ParseDirective(directiveNode, parser => parser.ReadImportDirectiveValue());

            BindingParserNode? alias = null;
            BindingParserNode? name;
            if (valueSyntaxRoot is BinaryOperatorBindingParserNode assignment)
            {
                alias = assignment.FirstExpression;
                name = assignment.SecondExpression;
            }
            else
            {
                name = valueSyntaxRoot;
            }

            return TreeBuilder.BuildImportDirective(directiveNode, alias, name);
        }

        protected override ImmutableList<NamespaceImport> CreateArtefact(IReadOnlyList<IAbstractImportDirective> directives)
            => ResolveNamespaceImportsCore(directives).ToImmutableList();

        private IEnumerable<NamespaceImport> ResolveNamespaceImportsCore(IReadOnlyList<IAbstractImportDirective> directives)
            => directives
            .Where(d => !d.HasError)
            .Select(d => new NamespaceImport(d.NameSyntax.ToDisplayString(), d.AliasSyntax.As<IdentifierNameBindingParserNode>()?.Name));
    }

}
