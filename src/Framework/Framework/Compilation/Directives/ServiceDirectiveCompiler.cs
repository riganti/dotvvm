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
    public class ServiceDirectiveCompiler : DirectiveCompiler<IAbstractServiceInjectDirective, ImmutableList<InjectedServiceExtensionParameter>>
    {
        public override string DirectiveName => ParserConstants.ServiceInjectDirective;

        public ServiceDirectiveCompiler(IReadOnlyDictionary<string, IReadOnlyList<DothtmlDirectiveNode>> directiveNodesByName, IAbstractTreeBuilder treeBuilder) : base(directiveNodesByName, treeBuilder)
        {
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
                return TreeBuilder.BuildServiceInjectDirective(directiveNode, name, type);
            }
            else
            {
                directiveNode.AddError($"Assignment operation expected - the correct form is `@{ParserConstants.ServiceInjectDirective} myStringService = ISomeService<string>`");
                return TreeBuilder.BuildServiceInjectDirective(directiveNode, new SimpleNameBindingParserNode("service"), valueSyntaxRoot);
            }
        }

        protected override ImmutableList<InjectedServiceExtensionParameter> CreateArtefact(IReadOnlyList<IAbstractServiceInjectDirective> directives) =>
            directives
            .Where(d => d.Type != null)
            .Select(d => new InjectedServiceExtensionParameter(d.NameSyntax.Name, d.Type!))
            .ToImmutableList();
    }

}
