using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation.Directives
{
    public class PropertyDeclarationDirectiveCompiler : DirectiveCompiler<IAbstractPropertyDeclarationDirective, ImmutableList<DotvvmProperty>>
    {
        private readonly ITypeDescriptor controlWrapperType;

        public override string DirectiveName => ParserConstants.PropertyDeclarationDirective;

        public PropertyDeclarationDirectiveCompiler(IReadOnlyDictionary<string, IReadOnlyList<DothtmlDirectiveNode>> directiveNodesByName, IAbstractTreeBuilder treeBuilder, ITypeDescriptor controlWrapperType)
            : base(directiveNodesByName, treeBuilder)
        {
            this.controlWrapperType = controlWrapperType;
        }

        protected override IAbstractPropertyDeclarationDirective Resolve(DothtmlDirectiveNode directiveNode)
        {
            var valueSyntaxRoot = ParseDirective(directiveNode, p => p.ReadPropertyDirectiveValue());

            var declaration = valueSyntaxRoot as PropertyDeclarationBindingParserNode;
            if (declaration == null)
            {
                directiveNode.AddError("Cannot resolve the property declaration.");
            }

            var type = declaration?.PropertyType as TypeReferenceBindingParserNode;
            if (type == null)
            {
                directiveNode.AddError($"Property type expected");
                type = new ActualTypeReferenceBindingParserNode(new SimpleNameBindingParserNode("string"));
            }

            var name = declaration?.Name as SimpleNameBindingParserNode;
            if (name == null)
            {
                directiveNode.AddError($"Property name expected.");
                name = new SimpleNameBindingParserNode("");
            }

            var attributeSyntaxes = (declaration?.Attributes ?? new List<BindingParserNode>());
            var resolvedAttributes = ProcessPropertyDirectiveAttributeReference(directiveNode, attributeSyntaxes)
                .Select(a => TreeBuilder.BuildPropertyDeclarationAttributeReference(directiveNode, a.name, a.type, a.initializer))
                .ToList();

            return TreeBuilder.BuildPropertyDeclarationDirective(directiveNode, type, name, declaration?.Initializer, resolvedAttributes, valueSyntaxRoot);
        }

        private List<(ActualTypeReferenceBindingParserNode type, IdentifierNameBindingParserNode name, LiteralExpressionBindingParserNode initializer)> ProcessPropertyDirectiveAttributeReference(DothtmlDirectiveNode directiveNode, List<BindingParserNode> attributeReferences)
        {
            var result = new List<(ActualTypeReferenceBindingParserNode, IdentifierNameBindingParserNode, LiteralExpressionBindingParserNode)>();
            foreach (var attributeReference in attributeReferences)
            {
                if (!(attributeReference is BinaryOperatorBindingParserNode assigment && assigment.Operator == BindingTokenType.AssignOperator))
                {
                    directiveNode.AddError("Property attributes must be in the form Attribute.Property = value.");
                    continue;
                }

                var attributePropertyReference = assigment.FirstExpression as MemberAccessBindingParserNode;
                var attributeTypeReference = attributePropertyReference?.TargetExpression;
                var attributePropertyNameReference = attributePropertyReference?.MemberNameExpression;
                var initializer = assigment.SecondExpression as LiteralExpressionBindingParserNode;

                if (attributeTypeReference == null || attributePropertyNameReference == null)
                {
                    directiveNode.AddError("Property attributes must be in the form Attribute.Property = value.");
                    continue;
                }
                if (initializer == null)
                {
                    directiveNode.AddError($"Value for property {attributeTypeReference.ToDisplayString()} of attribute {attributePropertyNameReference.ToDisplayString()} is missing or not a constant.");
                    continue;
                }
                result.Add((new ActualTypeReferenceBindingParserNode(attributeTypeReference), attributePropertyNameReference, initializer));
            }
            return result;
        }

        protected override ImmutableList<DotvvmProperty> CreateArtefact(IReadOnlyList<IAbstractPropertyDeclarationDirective> directives)
        {
            foreach (var directive in directives)
            {
                directive.DeclaringType = controlWrapperType;
            }

            return directives
            .Where(d => d.PropertyType is ResolvedTypeDescriptor resolvedTypeDescriptor && resolvedTypeDescriptor.Type is not null)
            .Select(d => TryCreateDotvvmPropertyFromDirective(d))
            .ToImmutableList();
        }

        protected virtual DotvvmProperty TryCreateDotvvmPropertyFromDirective(IAbstractPropertyDeclarationDirective propertyDeclarationDirective)
        {
            var propertyType = propertyDeclarationDirective.PropertyType.As<ResolvedTypeDescriptor>()!;
            var declaringType = propertyDeclarationDirective.DeclaringType.As<ResolvedTypeDescriptor>()!;

            return DotvvmProperty.Register(
                propertyDeclarationDirective.NameSyntax.Name,
                propertyType.Type,
                declaringType.Type,
                propertyDeclarationDirective.InitialValue,
                false,
                null,
                propertyDeclarationDirective,
                false);
        }
    }

}
