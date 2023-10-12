using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using System.Collections.Generic;
using System.Reflection;

namespace DotVVM.Framework.Compilation.ControlTree
{
    /// <summary>
    /// Abstract tree node representing semantic for control property declaration in DotHTML markup.
    ///
    /// Example:
    /// @property string MyProperty = "Initial value", MyAttribute.Property = true
    /// </summary>
    public interface IAbstractPropertyDeclarationDirective : IAbstractDirective, ICustomAttributeProvider
    {
        SimpleNameBindingParserNode NameSyntax { get; }
        TypeReferenceBindingParserNode PropertyTypeSyntax { get; }
        BindingParserNode? InitializerSyntax { get; }
        ITypeDescriptor? PropertyType { get; }
        ITypeDescriptor? DeclaringType { get; set; }
        object? InitialValue { get; }
        IList<IAbstractDirectiveAttributeReference> Attributes { get; }
    }

}
