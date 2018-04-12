using System.Collections.Immutable;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IMethodCallSyntax : IExpressionSyntax
    {
        IIdentifierSyntax Name { get; }
        ImmutableList<IExpressionSyntax> Parameters { get; }
    }
}
