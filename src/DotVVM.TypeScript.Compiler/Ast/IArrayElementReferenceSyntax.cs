using System;
using System.Text;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IArrayElementReferenceSyntax : IReferenceSyntax
    {
        IReferenceSyntax ArrayReference { get; }
        IExpressionSyntax ItemExpression { get; }
    }
}
