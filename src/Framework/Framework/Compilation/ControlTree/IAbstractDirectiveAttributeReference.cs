#nullable enable

using DotVVM.Framework.Compilation.Parser.Binding.Parser;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractDirectiveAttributeReference
    {
        public TypeReferenceBindingParserNode TypeSyntax { get; }
        public IdentifierNameBindingParserNode NameSyntax { get; }
        public LiteralExpressionBindingParserNode Initializer { get; }
        public ITypeDescriptor? Type { get; }
    }
}
