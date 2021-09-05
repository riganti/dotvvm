using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractServiceInjectDirective : IAbstractDirective
    {
        SimpleNameBindingParserNode NameSyntax { get; }
        BindingParserNode TypeSyntax { get; }
        ITypeDescriptor? Type { get; }
    }
}
