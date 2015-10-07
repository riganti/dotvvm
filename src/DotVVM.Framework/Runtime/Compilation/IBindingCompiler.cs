using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation
{
    public interface IBindingCompiler
    {
        ExpressionSyntax EmitCreateBinding(ResolvedBinding binding, string id, Type expectedType);
    }
}
