#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedServiceInjectDirective : ResolvedDirective, IAbstractServiceInjectDirective
    {
        public SimpleNameBindingParserNode NameSyntax { get; }
        public BindingParserNode TypeSyntax { get; }
        public Type? Type { get; }

        ITypeDescriptor? IAbstractServiceInjectDirective.Type => ResolvedTypeDescriptor.Create(Type);

        public ResolvedServiceInjectDirective(SimpleNameBindingParserNode nameSyntax, BindingParserNode typeSyntax, Type? injectedType)
        {
            NameSyntax = nameSyntax;
            TypeSyntax = typeSyntax;
            Type = injectedType;
        }
    }
}
