using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractDirective
    {
        DothtmlDirectiveNode DirectiveNode { get; }

        string Value { get; }

        IAbstractTreeRoot Parent { get; }
    }
}
