#nullable enable

using DotVVM.Framework.Compilation.Parser.Binding.Parser;

namespace DotVVM.Framework.Compilation.ControlTree
{
    /// <summary>
    /// Abstract tree node representing attribute defined on a markup declared property.
    /// See the part marked by `-- --` in th following next example:
    /// @property MyProp, --MyAttribute.Property--
    /// 
    /// This is used to provide simplyfied equivalent of `[MyAttribute]` C# syntax
    /// </summary>
    public interface IAbstractDirectiveAttributeReference
    {
        TypeReferenceBindingParserNode TypeSyntax { get; }
        IdentifierNameBindingParserNode NameSyntax { get; }
        LiteralExpressionBindingParserNode Initializer { get; }
        ITypeDescriptor? Type { get; }
    }
}
