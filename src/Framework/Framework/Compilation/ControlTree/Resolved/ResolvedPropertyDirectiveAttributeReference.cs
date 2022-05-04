using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedPropertyDirectiveAttributeReference : IAbstractDirectiveAttributeReference
    {
        public DothtmlDirectiveNode DirectiveNode { get; }
        public TypeReferenceBindingParserNode TypeSyntax { get; }
        public IdentifierNameBindingParserNode NameSyntax { get; }
        public ITypeDescriptor? Type { get; set; }
        public LiteralExpressionBindingParserNode Initializer { get; }

        public ResolvedPropertyDirectiveAttributeReference(
            DirectiveCompilationService directiveService,
            DothtmlDirectiveNode directiveNode,
            TypeReferenceBindingParserNode typeReferenceBindingParserNode,
            IdentifierNameBindingParserNode attributePropertyNameReference,
            LiteralExpressionBindingParserNode initializer,
            ImmutableList<NamespaceImport> imports)
        {
            DirectiveNode = directiveNode;
            TypeSyntax = typeReferenceBindingParserNode;
            NameSyntax = attributePropertyNameReference;
            Initializer = initializer;

            var typeDescriptor = directiveService.ResolveType(directiveNode, TypeSyntax, imports);

            if (typeDescriptor == null)
            {
                directiveNode.AddError($"Could not resolve type {TypeSyntax.ToDisplayString()} when trying to resolve property attribute type.");
            }
            Type = typeDescriptor;

        }
    }
}
