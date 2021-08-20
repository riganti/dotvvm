using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractTypeSpecificationDirective: IAbstractDirective
    {
        BindingParserNode NameSyntax { get; }
        ITypeDescriptor ResolvedType { get; }
    }
    public interface IAbstractViewModelDirective : IAbstractTypeSpecificationDirective { }
    public interface IAbstractBaseTypeDirective : IAbstractTypeSpecificationDirective { }
    public interface IAbstractPropertyDeclarationDirective : IAbstractDirective, ICustomAttributeProvider
    {
        SimpleNameBindingParserNode NameSyntax { get; }
        TypeReferenceBindingParserNode PropertyTypeSyntax { get; }
        ITypeDescriptor PropertyType { get; }
    }

}
