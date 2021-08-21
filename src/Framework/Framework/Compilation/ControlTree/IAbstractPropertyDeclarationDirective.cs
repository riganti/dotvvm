using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using System.Reflection;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractPropertyDeclarationDirective : IAbstractDirective, ICustomAttributeProvider
    {
        SimpleNameBindingParserNode NameSyntax { get; }
        TypeReferenceBindingParserNode PropertyTypeSyntax { get; }
        ITypeDescriptor PropertyType { get; }
        ITypeDescriptor? DeclaringType { get; set; }
        object? InitialValue { get; }
    }

}
