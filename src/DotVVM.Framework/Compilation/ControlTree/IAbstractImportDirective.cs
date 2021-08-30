using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractImportDirective : IAbstractDirective
    {
        BindingParserNode? AliasSyntax { get; }
        BindingParserNode NameSyntax { get; }

        bool IsNamespace { get; }
        bool IsType { get; }
        bool HasAlias { get; }
        bool HasError { get; }
    }
}
