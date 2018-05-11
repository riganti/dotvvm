using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IObjectCreationExpressionSyntax : IExpressionSyntax
    {
        ITypeSyntax ObjectType { get; }
        IList<IExpressionSyntax> Arguments { get; }
    }
}
