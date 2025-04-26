using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System;
using System.Text;
using System.Security.Cryptography;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.Directives
{
    public record PropertyDirectiveCompilerResult(ImmutableList<IPropertyDescriptor> Properties, ITypeDescriptor ModifiedMarkupControlType);

    public abstract class PropertyDeclarationDirectiveCompiler : DirectiveCompiler<IAbstractPropertyDeclarationDirective, PropertyDirectiveCompilerResult>
    {
        private readonly ITypeDescriptor controlWrapperType;
        private readonly ImmutableList<NamespaceImport> imports;

        protected virtual ITypeDescriptor DotvvmMarkupControlType => new ResolvedTypeDescriptor(typeof(DotvvmMarkupControl));

        public override string DirectiveName => ParserConstants.PropertyDeclarationDirective;

        public PropertyDeclarationDirectiveCompiler(ImmutableDictionary<string, ImmutableList<DothtmlDirectiveNode>> directiveNodesByName, IAbstractTreeBuilder treeBuilder, ITypeDescriptor controlWrapperType, ImmutableList<NamespaceImport> imports)
            : base(directiveNodesByName, treeBuilder)
        {
            this.controlWrapperType = controlWrapperType;
            this.imports = imports;
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
                .Select(a => TreeBuilder.BuildPropertyDeclarationAttributeReference(directiveNode, a.Name, a.Type, a.Initializer, imports))
                .ToList();

            return TreeBuilder.BuildPropertyDeclarationDirective(directiveNode, type, name, declaration?.Initializer, resolvedAttributes, valueSyntaxRoot, imports);
        }

        private List<AttributeInfo> ProcessPropertyDirectiveAttributeReference(DothtmlDirectiveNode directiveNode, List<BindingParserNode> attributeReferences)
        {
            return attributeReferences.Select(attr => GetAttributeInfo(attr, directiveNode)).ToList();
        }

        private AttributeInfo GetAttributeInfo(BindingParserNode attributeReference, DothtmlDirectiveNode directiveNode)
        {
            if (attributeReference is not BinaryOperatorBindingParserNode { Operator: BindingTokenType.AssignOperator } assignment)
            {
                directiveNode.AddError("Property attributes must be in the form Attribute.Property = value.");
                //No idea what is that, lets make it a type and move on

                var typeRef = new ActualTypeReferenceBindingParserNode(attributeReference);
                typeRef.TransferTokens(attributeReference);

                return new AttributeInfo(
                    typeRef,
                    new SimpleNameBindingParserNode("") { StartPosition = attributeReference.EndPosition },
                    new SimpleNameBindingParserNode("") { StartPosition = attributeReference.EndPosition });
            }

            var attributePropertyReference = assignment.FirstExpression as MemberAccessBindingParserNode;
            var initializer = assignment.SecondExpression; 
            var attributeTypeReference = attributePropertyReference?.TargetExpression;
            var attributePropertyNameReference = attributePropertyReference?.MemberNameExpression;


            if (attributeTypeReference is null || attributePropertyNameReference is null)
            {
                directiveNode.AddError("Property attributes must be in the form Attribute.Property = value.");
                //Name is probably mising or type is incomplete
                attributeTypeReference = attributeTypeReference ?? new SimpleNameBindingParserNode("");
                attributePropertyNameReference = attributePropertyNameReference ?? new SimpleNameBindingParserNode("") { StartPosition = attributeTypeReference.EndPosition };
            }
            if (assignment.SecondExpression is not LiteralExpressionBindingParserNode)
            {
                directiveNode.AddError($"Value for property {attributeTypeReference.ToDisplayString()} of attribute {attributePropertyNameReference.ToDisplayString()} is missing or not a constant.");
            }

            var type = new ActualTypeReferenceBindingParserNode(attributeTypeReference);
            type.TransferTokens(attributeTypeReference);
            return new AttributeInfo(type, attributePropertyNameReference, initializer);
        }

        protected override PropertyDirectiveCompilerResult CreateArtefact(ImmutableList<IAbstractPropertyDeclarationDirective> directives)
        {
            var generatedWrapperType = directives.Any()
                    ? (CreateDynamicDeclaringType(controlWrapperType, directives) ?? controlWrapperType)
                    : controlWrapperType;

            foreach (var directive in directives)
            {
                directive.DeclaringType = generatedWrapperType;
            }

            var properties = directives
            .Where(HasPropertyType)
            .GroupBy(
                directive => directive.NameSyntax.Name,
                directive => directive,
                (name, directiveOfSameName) => {
                    if (directiveOfSameName.Count() > 1)
                    {
                        foreach (var sameNameDirective in directiveOfSameName.Skip(1))
                        {
                            sameNameDirective.DothtmlNode?.AddError("Property with the same name is already defined.");
                        };
                    }
                    return directiveOfSameName.First();
                })
            .Select(TryCreateDotvvmPropertyFromDirective)
            .ToImmutableList();

            return new PropertyDirectiveCompilerResult(properties, generatedWrapperType);
        }

        /// <summary> Gets or creates dynamic declaring type, and registers on it the properties declared using `@property` directives </summary>
        protected virtual ITypeDescriptor? CreateDynamicDeclaringType(
            ITypeDescriptor? originalWrapperType,
            ImmutableList<IAbstractPropertyDeclarationDirective> propertyDirectives
        )
        {
            var imports = DirectiveNodesByName.GetValueOrDefault(ParserConstants.ImportNamespaceDirective, ImmutableList<DothtmlDirectiveNode>.Empty)
                .Select(d => d.Value.Trim()).OrderBy(s => s).ToImmutableArray();
            var properties = propertyDirectives
                .Select(p => p.Value.Trim()).OrderBy(s => s).ToImmutableArray();
            var baseType = originalWrapperType ?? DotvvmMarkupControlType;

            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(
                new UTF8Encoding(false).GetBytes(
                    baseType.FullName + "||" + string.Join("|", imports) + "||" + string.Join("|", properties)
                )
            );
            var hash = Convert.ToBase64String(hashBytes, 0, 16)
                .Replace('+', '_')
                .Replace('/', '_')
                .TrimEnd('=');
            var typeName = "DotvvmMarkupControl-" + hash;

            return GetOrCreateDynamicType(baseType, typeName, propertyDirectives);
        }

        protected abstract ITypeDescriptor? GetOrCreateDynamicType(ITypeDescriptor baseType, string typeName, ImmutableList<IAbstractPropertyDeclarationDirective> propertyDirectives);

        protected abstract bool HasPropertyType(IAbstractPropertyDeclarationDirective directive);
        protected abstract IPropertyDescriptor TryCreateDotvvmPropertyFromDirective(IAbstractPropertyDeclarationDirective propertyDeclarationDirective);

        private record AttributeInfo(ActualTypeReferenceBindingParserNode Type, IdentifierNameBindingParserNode Name, BindingParserNode Initializer);
    }
}
