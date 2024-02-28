using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation.Directives
{
    using DirectiveDictionary = ImmutableDictionary<string, ImmutableList<DothtmlDirectiveNode>>;

    public class ServiceDirectiveCompiler : DirectiveCompiler<IAbstractServiceInjectDirective, ImmutableList<InjectedServiceExtensionParameter>>
    {
        private readonly ImmutableList<NamespaceImport> imports;

        public override string DirectiveName => ParserConstants.ServiceInjectDirective;

        public ServiceDirectiveCompiler(DirectiveDictionary directiveNodesByName, IAbstractTreeBuilder treeBuilder, ImmutableList<NamespaceImport> imports)
            : base(directiveNodesByName, treeBuilder)
        {
            this.imports = imports;
        }

        protected override IAbstractServiceInjectDirective Resolve(DothtmlDirectiveNode directiveNode)
        {
            var valueSyntaxRoot = ParseDirective(directiveNode, parser => parser.ReadImportDirectiveValue());

            if (valueSyntaxRoot is BinaryOperatorBindingParserNode assignment)
            {
                var name = assignment.FirstExpression as SimpleNameBindingParserNode;
                if (name == null)
                {
                    directiveNode.AddError($"Identifier expected on the left side of the assignment.");
                    name = new SimpleNameBindingParserNode("service");
                }
                var type = assignment.SecondExpression;
                return TreeBuilder.BuildServiceInjectDirective(directiveNode, name, type, imports);
            }
            else
            {
                directiveNode.AddError($"Assignment operation expected - the correct form is `@{ParserConstants.ServiceInjectDirective} myStringService = ISomeService<string>`");
                return TreeBuilder.BuildServiceInjectDirective(directiveNode, new SimpleNameBindingParserNode("service"), valueSyntaxRoot, imports);
            }
        }

        protected override ImmutableList<InjectedServiceExtensionParameter> CreateArtefact(ImmutableList<IAbstractServiceInjectDirective> directives) =>
            directives
            .Where(d => d.Type != null)
            .Select(d => new InjectedServiceExtensionParameter(d.NameSyntax.Name, d.Type!))
            .ToImmutableList();
    }

}
