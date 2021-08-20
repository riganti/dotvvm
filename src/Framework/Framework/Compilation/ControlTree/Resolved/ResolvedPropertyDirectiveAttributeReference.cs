using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedPropertyDirectiveAttributeReference : IAbstractDirectiveAttributeReference
    {
        public TypeReferenceBindingParserNode TypeSyntax { get; }
        public IdentifierNameBindingParserNode NameSyntax { get; }
        public ITypeDescriptor Type { get; set; }
        public LiteralExpressionBindingParserNode Initializer { get; }

        public ResolvedPropertyDirectiveAttributeReference(
            TypeReferenceBindingParserNode typeReferenceBindingParserNode,
            IdentifierNameBindingParserNode attributePropertyNameReference,
            ResolvedTypeDescriptor typeDescriptor,
            LiteralExpressionBindingParserNode initializer)
        {
            TypeSyntax = typeReferenceBindingParserNode;
            NameSyntax = attributePropertyNameReference;
            Type = typeDescriptor;
            Initializer = initializer;
        }
    }
}
